/*

   GENERATED SERVICE FOR THE MESSAGEFLAG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MessageFlag table.

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
export class MessageFlagQueryParameters {
    conversationMessageId: bigint | number | null | undefined = null;
    flaggedByUserId: bigint | number | null | undefined = null;
    reason: string | null | undefined = null;
    details: string | null | undefined = null;
    status: string | null | undefined = null;
    reviewedByUserId: bigint | number | null | undefined = null;
    dateTimeReviewed: string | null | undefined = null;        // ISO 8601 (full datetime)
    resolutionNotes: string | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class MessageFlagSubmitData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    flaggedByUserId!: bigint | number;
    reason!: string;
    details: string | null = null;
    status!: string;
    reviewedByUserId: bigint | number | null = null;
    dateTimeReviewed: string | null = null;     // ISO 8601 (full datetime)
    resolutionNotes: string | null = null;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class MessageFlagBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MessageFlagChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `messageFlag.MessageFlagChildren$` — use with `| async` in templates
//        • Promise:    `messageFlag.MessageFlagChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="messageFlag.MessageFlagChildren$ | async"`), or
//        • Access the promise getter (`messageFlag.MessageFlagChildren` or `await messageFlag.MessageFlagChildren`)
//    - Simply reading `messageFlag.MessageFlagChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await messageFlag.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MessageFlagData {
    id!: bigint | number;
    conversationMessageId!: bigint | number;
    flaggedByUserId!: bigint | number;
    reason!: string;
    details!: string | null;
    status!: string;
    reviewedByUserId!: bigint | number;
    dateTimeReviewed!: string | null;   // ISO 8601 (full datetime)
    resolutionNotes!: string | null;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
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
  // Promise based reload method to allow rebuilding of any MessageFlagData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.messageFlag.Reload();
  //
  //  Non Async:
  //
  //     messageFlag[0].Reload().then(x => {
  //        this.messageFlag = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MessageFlagService.Instance.GetMessageFlag(this.id, includeRelations)
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
     * Updates the state of this MessageFlagData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MessageFlagData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MessageFlagSubmitData {
        return MessageFlagService.Instance.ConvertToMessageFlagSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MessageFlagService extends SecureEndpointBase {

    private static _instance: MessageFlagService;
    private listCache: Map<string, Observable<Array<MessageFlagData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MessageFlagBasicListData>>>;
    private recordCache: Map<string, Observable<MessageFlagData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MessageFlagData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MessageFlagBasicListData>>>();
        this.recordCache = new Map<string, Observable<MessageFlagData>>();

        MessageFlagService._instance = this;
    }

    public static get Instance(): MessageFlagService {
      return MessageFlagService._instance;
    }


    public ClearListCaches(config: MessageFlagQueryParameters | null = null) {

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


    public ConvertToMessageFlagSubmitData(data: MessageFlagData): MessageFlagSubmitData {

        let output = new MessageFlagSubmitData();

        output.id = data.id;
        output.conversationMessageId = data.conversationMessageId;
        output.flaggedByUserId = data.flaggedByUserId;
        output.reason = data.reason;
        output.details = data.details;
        output.status = data.status;
        output.reviewedByUserId = data.reviewedByUserId;
        output.dateTimeReviewed = data.dateTimeReviewed;
        output.resolutionNotes = data.resolutionNotes;
        output.dateTimeCreated = data.dateTimeCreated;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetMessageFlag(id: bigint | number, includeRelations: boolean = true) : Observable<MessageFlagData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const messageFlag$ = this.requestMessageFlag(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MessageFlag", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, messageFlag$);

            return messageFlag$;
        }

        return this.recordCache.get(configHash) as Observable<MessageFlagData>;
    }

    private requestMessageFlag(id: bigint | number, includeRelations: boolean = true) : Observable<MessageFlagData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MessageFlagData>(this.baseUrl + 'api/MessageFlag/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMessageFlag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMessageFlag(id, includeRelations));
            }));
    }

    public GetMessageFlagList(config: MessageFlagQueryParameters | any = null) : Observable<Array<MessageFlagData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const messageFlagList$ = this.requestMessageFlagList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MessageFlag list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, messageFlagList$);

            return messageFlagList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MessageFlagData>>;
    }


    private requestMessageFlagList(config: MessageFlagQueryParameters | any) : Observable <Array<MessageFlagData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MessageFlagData>>(this.baseUrl + 'api/MessageFlags', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMessageFlagList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMessageFlagList(config));
            }));
    }

    public GetMessageFlagsRowCount(config: MessageFlagQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const messageFlagsRowCount$ = this.requestMessageFlagsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MessageFlags row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, messageFlagsRowCount$);

            return messageFlagsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMessageFlagsRowCount(config: MessageFlagQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MessageFlags/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMessageFlagsRowCount(config));
            }));
    }

    public GetMessageFlagsBasicListData(config: MessageFlagQueryParameters | any = null) : Observable<Array<MessageFlagBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const messageFlagsBasicListData$ = this.requestMessageFlagsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MessageFlags basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, messageFlagsBasicListData$);

            return messageFlagsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MessageFlagBasicListData>>;
    }


    private requestMessageFlagsBasicListData(config: MessageFlagQueryParameters | any) : Observable<Array<MessageFlagBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MessageFlagBasicListData>>(this.baseUrl + 'api/MessageFlags/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMessageFlagsBasicListData(config));
            }));

    }


    public PutMessageFlag(id: bigint | number, messageFlag: MessageFlagSubmitData) : Observable<MessageFlagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MessageFlagData>(this.baseUrl + 'api/MessageFlag/' + id.toString(), messageFlag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMessageFlag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMessageFlag(id, messageFlag));
            }));
    }


    public PostMessageFlag(messageFlag: MessageFlagSubmitData) : Observable<MessageFlagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MessageFlagData>(this.baseUrl + 'api/MessageFlag', messageFlag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMessageFlag(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMessageFlag(messageFlag));
            }));
    }

  
    public DeleteMessageFlag(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MessageFlag/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMessageFlag(id));
            }));
    }


    private getConfigHash(config: MessageFlagQueryParameters | any): string {

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

    public userIsSchedulerMessageFlagReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerMessageFlagReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.MessageFlags
        //
        if (userIsSchedulerMessageFlagReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerMessageFlagReader = user.readPermission >= 50;
            } else {
                userIsSchedulerMessageFlagReader = false;
            }
        }

        return userIsSchedulerMessageFlagReader;
    }


    public userIsSchedulerMessageFlagWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerMessageFlagWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.MessageFlags
        //
        if (userIsSchedulerMessageFlagWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerMessageFlagWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerMessageFlagWriter = false;
          }      
        }

        return userIsSchedulerMessageFlagWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MessageFlagData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MessageFlagData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MessageFlagTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMessageFlag(raw: any): MessageFlagData {
    if (!raw) return raw;

    //
    // Create a MessageFlagData object instance with correct prototype
    //
    const revived = Object.create(MessageFlagData.prototype) as MessageFlagData;

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
    // 2. But private methods (loadMessageFlagXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveMessageFlagList(rawList: any[]): MessageFlagData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMessageFlag(raw));
  }

}
