/*

   GENERATED SERVICE FOR THE CONVERSATIONMESSAGEUSER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConversationMessageUser table.

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
import { ConversationMessageData } from './conversation-message.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConversationMessageUserQueryParameters {
    conversationMessageId: bigint | number | null | undefined = null;
    userId: bigint | number | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
    acknowledged: boolean | null | undefined = null;
    dateTimeAcknowledged: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class ConversationMessageUserSubmitData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    userId!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    acknowledged!: boolean;
    dateTimeAcknowledged!: string;      // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class ConversationMessageUserBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConversationMessageUserChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `conversationMessageUser.ConversationMessageUserChildren$` — use with `| async` in templates
//        • Promise:    `conversationMessageUser.ConversationMessageUserChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="conversationMessageUser.ConversationMessageUserChildren$ | async"`), or
//        • Access the promise getter (`conversationMessageUser.ConversationMessageUserChildren` or `await conversationMessageUser.ConversationMessageUserChildren`)
//    - Simply reading `conversationMessageUser.ConversationMessageUserChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await conversationMessageUser.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConversationMessageUserData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    userId!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    acknowledged!: boolean;
    dateTimeAcknowledged!: string;      // ISO 8601 (full datetime)
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    conversationMessage: ConversationMessageData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ConversationMessageUserData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.conversationMessageUser.Reload();
  //
  //  Non Async:
  //
  //     conversationMessageUser[0].Reload().then(x => {
  //        this.conversationMessageUser = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConversationMessageUserService.Instance.GetConversationMessageUser(this.id, includeRelations)
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
     * Updates the state of this ConversationMessageUserData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConversationMessageUserData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConversationMessageUserSubmitData {
        return ConversationMessageUserService.Instance.ConvertToConversationMessageUserSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConversationMessageUserService extends SecureEndpointBase {

    private static _instance: ConversationMessageUserService;
    private listCache: Map<string, Observable<Array<ConversationMessageUserData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConversationMessageUserBasicListData>>>;
    private recordCache: Map<string, Observable<ConversationMessageUserData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConversationMessageUserData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConversationMessageUserBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConversationMessageUserData>>();

        ConversationMessageUserService._instance = this;
    }

    public static get Instance(): ConversationMessageUserService {
      return ConversationMessageUserService._instance;
    }


    public ClearListCaches(config: ConversationMessageUserQueryParameters | null = null) {

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


    public ConvertToConversationMessageUserSubmitData(data: ConversationMessageUserData): ConversationMessageUserSubmitData {

        let output = new ConversationMessageUserSubmitData();

        output.id = data.id;
        output.conversationMessageId = data.conversationMessageId;
        output.userId = data.userId;
        output.dateTimeCreated = data.dateTimeCreated;
        output.acknowledged = data.acknowledged;
        output.dateTimeAcknowledged = data.dateTimeAcknowledged;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConversationMessageUser(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageUserData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const conversationMessageUser$ = this.requestConversationMessageUser(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageUser", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, conversationMessageUser$);

            return conversationMessageUser$;
        }

        return this.recordCache.get(configHash) as Observable<ConversationMessageUserData>;
    }

    private requestConversationMessageUser(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageUserData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageUserData>(this.baseUrl + 'api/ConversationMessageUser/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConversationMessageUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageUser(id, includeRelations));
            }));
    }

    public GetConversationMessageUserList(config: ConversationMessageUserQueryParameters | any = null) : Observable<Array<ConversationMessageUserData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const conversationMessageUserList$ = this.requestConversationMessageUserList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageUser list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, conversationMessageUserList$);

            return conversationMessageUserList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConversationMessageUserData>>;
    }


    private requestConversationMessageUserList(config: ConversationMessageUserQueryParameters | any) : Observable <Array<ConversationMessageUserData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageUserData>>(this.baseUrl + 'api/ConversationMessageUsers', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConversationMessageUserList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageUserList(config));
            }));
    }

    public GetConversationMessageUsersRowCount(config: ConversationMessageUserQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const conversationMessageUsersRowCount$ = this.requestConversationMessageUsersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageUsers row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, conversationMessageUsersRowCount$);

            return conversationMessageUsersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConversationMessageUsersRowCount(config: ConversationMessageUserQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConversationMessageUsers/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageUsersRowCount(config));
            }));
    }

    public GetConversationMessageUsersBasicListData(config: ConversationMessageUserQueryParameters | any = null) : Observable<Array<ConversationMessageUserBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const conversationMessageUsersBasicListData$ = this.requestConversationMessageUsersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessageUsers basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, conversationMessageUsersBasicListData$);

            return conversationMessageUsersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConversationMessageUserBasicListData>>;
    }


    private requestConversationMessageUsersBasicListData(config: ConversationMessageUserQueryParameters | any) : Observable<Array<ConversationMessageUserBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageUserBasicListData>>(this.baseUrl + 'api/ConversationMessageUsers/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageUsersBasicListData(config));
            }));

    }


    public PutConversationMessageUser(id: bigint | number, conversationMessageUser: ConversationMessageUserSubmitData) : Observable<ConversationMessageUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationMessageUserData>(this.baseUrl + 'api/ConversationMessageUser/' + id.toString(), conversationMessageUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConversationMessageUser(id, conversationMessageUser));
            }));
    }


    public PostConversationMessageUser(conversationMessageUser: ConversationMessageUserSubmitData) : Observable<ConversationMessageUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConversationMessageUserData>(this.baseUrl + 'api/ConversationMessageUser', conversationMessageUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessageUser(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConversationMessageUser(conversationMessageUser));
            }));
    }

  
    public DeleteConversationMessageUser(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConversationMessageUser/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConversationMessageUser(id));
            }));
    }


    private getConfigHash(config: ConversationMessageUserQueryParameters | any): string {

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

    public userIsSchedulerConversationMessageUserReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConversationMessageUserReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ConversationMessageUsers
        //
        if (userIsSchedulerConversationMessageUserReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConversationMessageUserReader = user.readPermission >= 50;
            } else {
                userIsSchedulerConversationMessageUserReader = false;
            }
        }

        return userIsSchedulerConversationMessageUserReader;
    }


    public userIsSchedulerConversationMessageUserWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConversationMessageUserWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ConversationMessageUsers
        //
        if (userIsSchedulerConversationMessageUserWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConversationMessageUserWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerConversationMessageUserWriter = false;
          }      
        }

        return userIsSchedulerConversationMessageUserWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ConversationMessageUserData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConversationMessageUserData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConversationMessageUserTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConversationMessageUser(raw: any): ConversationMessageUserData {
    if (!raw) return raw;

    //
    // Create a ConversationMessageUserData object instance with correct prototype
    //
    const revived = Object.create(ConversationMessageUserData.prototype) as ConversationMessageUserData;

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
    // 2. But private methods (loadConversationMessageUserXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveConversationMessageUserList(rawList: any[]): ConversationMessageUserData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConversationMessageUser(raw));
  }

}
