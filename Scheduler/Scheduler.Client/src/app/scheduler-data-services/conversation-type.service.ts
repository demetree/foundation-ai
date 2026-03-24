/*

   GENERATED SERVICE FOR THE CONVERSATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConversationType table.

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
import { ConversationService, ConversationData } from './conversation.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConversationTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class ConversationTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ConversationTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConversationTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `conversationType.ConversationTypeChildren$` — use with `| async` in templates
//        • Promise:    `conversationType.ConversationTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="conversationType.ConversationTypeChildren$ | async"`), or
//        • Access the promise getter (`conversationType.ConversationTypeChildren` or `await conversationType.ConversationTypeChildren`)
//    - Simply reading `conversationType.ConversationTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await conversationType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConversationTypeData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _conversations: ConversationData[] | null = null;
    private _conversationsPromise: Promise<ConversationData[]> | null  = null;
    private _conversationsSubject = new BehaviorSubject<ConversationData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Conversations$ = this._conversationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversations === null && this._conversationsPromise === null) {
            this.loadConversations(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationsCount$: Observable<bigint | number> | null = null;
    public get ConversationsCount$(): Observable<bigint | number> {
        if (this._conversationsCount$ === null) {
            this._conversationsCount$ = ConversationService.Instance.GetConversationsRowCount({conversationTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConversationTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.conversationType.Reload();
  //
  //  Non Async:
  //
  //     conversationType[0].Reload().then(x => {
  //        this.conversationType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConversationTypeService.Instance.GetConversationType(this.id, includeRelations)
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
     this._conversations = null;
     this._conversationsPromise = null;
     this._conversationsSubject.next(null);
     this._conversationsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Conversations for this ConversationType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationType.Conversations.then(conversationTypes => { ... })
     *   or
     *   await this.conversationType.conversationTypes
     *
    */
    public get Conversations(): Promise<ConversationData[]> {
        if (this._conversations !== null) {
            return Promise.resolve(this._conversations);
        }

        if (this._conversationsPromise !== null) {
            return this._conversationsPromise;
        }

        // Start the load
        this.loadConversations();

        return this._conversationsPromise!;
    }



    private loadConversations(): void {

        this._conversationsPromise = lastValueFrom(
            ConversationTypeService.Instance.GetConversationsForConversationType(this.id)
        )
        .then(Conversations => {
            this._conversations = Conversations ?? [];
            this._conversationsSubject.next(this._conversations);
            return this._conversations;
         })
        .catch(err => {
            this._conversations = [];
            this._conversationsSubject.next(this._conversations);
            throw err;
        })
        .finally(() => {
            this._conversationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Conversation. Call after mutations to force refresh.
     */
    public ClearConversationsCache(): void {
        this._conversations = null;
        this._conversationsPromise = null;
        this._conversationsSubject.next(this._conversations);      // Emit to observable
    }

    public get HasConversations(): Promise<boolean> {
        return this.Conversations.then(conversations => conversations.length > 0);
    }




    /**
     * Updates the state of this ConversationTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConversationTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConversationTypeSubmitData {
        return ConversationTypeService.Instance.ConvertToConversationTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConversationTypeService extends SecureEndpointBase {

    private static _instance: ConversationTypeService;
    private listCache: Map<string, Observable<Array<ConversationTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConversationTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ConversationTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private conversationService: ConversationService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConversationTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConversationTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConversationTypeData>>();

        ConversationTypeService._instance = this;
    }

    public static get Instance(): ConversationTypeService {
      return ConversationTypeService._instance;
    }


    public ClearListCaches(config: ConversationTypeQueryParameters | null = null) {

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


    public ConvertToConversationTypeSubmitData(data: ConversationTypeData): ConversationTypeSubmitData {

        let output = new ConversationTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConversationType(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const conversationType$ = this.requestConversationType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, conversationType$);

            return conversationType$;
        }

        return this.recordCache.get(configHash) as Observable<ConversationTypeData>;
    }

    private requestConversationType(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationTypeData>(this.baseUrl + 'api/ConversationType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConversationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationType(id, includeRelations));
            }));
    }

    public GetConversationTypeList(config: ConversationTypeQueryParameters | any = null) : Observable<Array<ConversationTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const conversationTypeList$ = this.requestConversationTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, conversationTypeList$);

            return conversationTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConversationTypeData>>;
    }


    private requestConversationTypeList(config: ConversationTypeQueryParameters | any) : Observable <Array<ConversationTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationTypeData>>(this.baseUrl + 'api/ConversationTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConversationTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationTypeList(config));
            }));
    }

    public GetConversationTypesRowCount(config: ConversationTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const conversationTypesRowCount$ = this.requestConversationTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, conversationTypesRowCount$);

            return conversationTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConversationTypesRowCount(config: ConversationTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConversationTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationTypesRowCount(config));
            }));
    }

    public GetConversationTypesBasicListData(config: ConversationTypeQueryParameters | any = null) : Observable<Array<ConversationTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const conversationTypesBasicListData$ = this.requestConversationTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, conversationTypesBasicListData$);

            return conversationTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConversationTypeBasicListData>>;
    }


    private requestConversationTypesBasicListData(config: ConversationTypeQueryParameters | any) : Observable<Array<ConversationTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationTypeBasicListData>>(this.baseUrl + 'api/ConversationTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationTypesBasicListData(config));
            }));

    }


    public PutConversationType(id: bigint | number, conversationType: ConversationTypeSubmitData) : Observable<ConversationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationTypeData>(this.baseUrl + 'api/ConversationType/' + id.toString(), conversationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConversationType(id, conversationType));
            }));
    }


    public PostConversationType(conversationType: ConversationTypeSubmitData) : Observable<ConversationTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConversationTypeData>(this.baseUrl + 'api/ConversationType', conversationType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConversationType(conversationType));
            }));
    }

  
    public DeleteConversationType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConversationType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConversationType(id));
            }));
    }


    private getConfigHash(config: ConversationTypeQueryParameters | any): string {

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

    public userIsSchedulerConversationTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConversationTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ConversationTypes
        //
        if (userIsSchedulerConversationTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConversationTypeReader = user.readPermission >= 100;
            } else {
                userIsSchedulerConversationTypeReader = false;
            }
        }

        return userIsSchedulerConversationTypeReader;
    }


    public userIsSchedulerConversationTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConversationTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ConversationTypes
        //
        if (userIsSchedulerConversationTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConversationTypeWriter = user.writePermission >= 100;
          } else {
            userIsSchedulerConversationTypeWriter = false;
          }      
        }

        return userIsSchedulerConversationTypeWriter;
    }

    public GetConversationsForConversationType(conversationTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationData[]> {
        return this.conversationService.GetConversationList({
            conversationTypeId: conversationTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConversationTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConversationTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConversationTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConversationType(raw: any): ConversationTypeData {
    if (!raw) return raw;

    //
    // Create a ConversationTypeData object instance with correct prototype
    //
    const revived = Object.create(ConversationTypeData.prototype) as ConversationTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._conversations = null;
    (revived as any)._conversationsPromise = null;
    (revived as any)._conversationsSubject = new BehaviorSubject<ConversationData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConversationTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Conversations$ = (revived as any)._conversationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversations === null && (revived as any)._conversationsPromise === null) {
                (revived as any).loadConversations();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationsCount$ = null;



    return revived;
  }

  private ReviveConversationTypeList(rawList: any[]): ConversationTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConversationType(raw));
  }

}
