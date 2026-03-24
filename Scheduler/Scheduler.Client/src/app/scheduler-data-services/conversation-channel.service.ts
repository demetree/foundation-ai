/*

   GENERATED SERVICE FOR THE CONVERSATIONCHANNEL TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConversationChannel table.

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
import { ConversationData } from './conversation.service';
import { ConversationChannelChangeHistoryService, ConversationChannelChangeHistoryData } from './conversation-channel-change-history.service';
import { ConversationMessageService, ConversationMessageData } from './conversation-message.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConversationChannelQueryParameters {
    conversationId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    topic: string | null | undefined = null;
    isPrivate: boolean | null | undefined = null;
    isPinned: boolean | null | undefined = null;
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
export class ConversationChannelSubmitData {
    id!: bigint | number;
    conversationId!: bigint | number;
    name!: string;
    topic: string | null = null;
    isPrivate!: boolean;
    isPinned!: boolean;
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

export class ConversationChannelBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConversationChannelChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `conversationChannel.ConversationChannelChildren$` — use with `| async` in templates
//        • Promise:    `conversationChannel.ConversationChannelChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="conversationChannel.ConversationChannelChildren$ | async"`), or
//        • Access the promise getter (`conversationChannel.ConversationChannelChildren` or `await conversationChannel.ConversationChannelChildren`)
//    - Simply reading `conversationChannel.ConversationChannelChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await conversationChannel.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConversationChannelData {
    id!: bigint | number;
    conversationId!: bigint | number;
    name!: string;
    topic!: string | null;
    isPrivate!: boolean;
    isPinned!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    conversation: ConversationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _conversationChannelChangeHistories: ConversationChannelChangeHistoryData[] | null = null;
    private _conversationChannelChangeHistoriesPromise: Promise<ConversationChannelChangeHistoryData[]> | null  = null;
    private _conversationChannelChangeHistoriesSubject = new BehaviorSubject<ConversationChannelChangeHistoryData[] | null>(null);

                
    private _conversationMessages: ConversationMessageData[] | null = null;
    private _conversationMessagesPromise: Promise<ConversationMessageData[]> | null  = null;
    private _conversationMessagesSubject = new BehaviorSubject<ConversationMessageData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ConversationChannelData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ConversationChannelData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationChannelData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConversationChannelChangeHistories$ = this._conversationChannelChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationChannelChangeHistories === null && this._conversationChannelChangeHistoriesPromise === null) {
            this.loadConversationChannelChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationChannelChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ConversationChannelChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._conversationChannelChangeHistoriesCount$ === null) {
            this._conversationChannelChangeHistoriesCount$ = ConversationChannelChangeHistoryService.Instance.GetConversationChannelChangeHistoriesRowCount({conversationChannelId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationChannelChangeHistoriesCount$;
    }



    public ConversationMessages$ = this._conversationMessagesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessages === null && this._conversationMessagesPromise === null) {
            this.loadConversationMessages(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessagesCount$: Observable<bigint | number> | null = null;
    public get ConversationMessagesCount$(): Observable<bigint | number> {
        if (this._conversationMessagesCount$ === null) {
            this._conversationMessagesCount$ = ConversationMessageService.Instance.GetConversationMessagesRowCount({conversationChannelId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessagesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConversationChannelData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.conversationChannel.Reload();
  //
  //  Non Async:
  //
  //     conversationChannel[0].Reload().then(x => {
  //        this.conversationChannel = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConversationChannelService.Instance.GetConversationChannel(this.id, includeRelations)
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
     this._conversationChannelChangeHistories = null;
     this._conversationChannelChangeHistoriesPromise = null;
     this._conversationChannelChangeHistoriesSubject.next(null);
     this._conversationChannelChangeHistoriesCount$ = null;

     this._conversationMessages = null;
     this._conversationMessagesPromise = null;
     this._conversationMessagesSubject.next(null);
     this._conversationMessagesCount$ = null;

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
     * Gets the ConversationChannelChangeHistories for this ConversationChannel.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationChannel.ConversationChannelChangeHistories.then(conversationChannels => { ... })
     *   or
     *   await this.conversationChannel.conversationChannels
     *
    */
    public get ConversationChannelChangeHistories(): Promise<ConversationChannelChangeHistoryData[]> {
        if (this._conversationChannelChangeHistories !== null) {
            return Promise.resolve(this._conversationChannelChangeHistories);
        }

        if (this._conversationChannelChangeHistoriesPromise !== null) {
            return this._conversationChannelChangeHistoriesPromise;
        }

        // Start the load
        this.loadConversationChannelChangeHistories();

        return this._conversationChannelChangeHistoriesPromise!;
    }



    private loadConversationChannelChangeHistories(): void {

        this._conversationChannelChangeHistoriesPromise = lastValueFrom(
            ConversationChannelService.Instance.GetConversationChannelChangeHistoriesForConversationChannel(this.id)
        )
        .then(ConversationChannelChangeHistories => {
            this._conversationChannelChangeHistories = ConversationChannelChangeHistories ?? [];
            this._conversationChannelChangeHistoriesSubject.next(this._conversationChannelChangeHistories);
            return this._conversationChannelChangeHistories;
         })
        .catch(err => {
            this._conversationChannelChangeHistories = [];
            this._conversationChannelChangeHistoriesSubject.next(this._conversationChannelChangeHistories);
            throw err;
        })
        .finally(() => {
            this._conversationChannelChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationChannelChangeHistory. Call after mutations to force refresh.
     */
    public ClearConversationChannelChangeHistoriesCache(): void {
        this._conversationChannelChangeHistories = null;
        this._conversationChannelChangeHistoriesPromise = null;
        this._conversationChannelChangeHistoriesSubject.next(this._conversationChannelChangeHistories);      // Emit to observable
    }

    public get HasConversationChannelChangeHistories(): Promise<boolean> {
        return this.ConversationChannelChangeHistories.then(conversationChannelChangeHistories => conversationChannelChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ConversationMessages for this ConversationChannel.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationChannel.ConversationMessages.then(conversationChannels => { ... })
     *   or
     *   await this.conversationChannel.conversationChannels
     *
    */
    public get ConversationMessages(): Promise<ConversationMessageData[]> {
        if (this._conversationMessages !== null) {
            return Promise.resolve(this._conversationMessages);
        }

        if (this._conversationMessagesPromise !== null) {
            return this._conversationMessagesPromise;
        }

        // Start the load
        this.loadConversationMessages();

        return this._conversationMessagesPromise!;
    }



    private loadConversationMessages(): void {

        this._conversationMessagesPromise = lastValueFrom(
            ConversationChannelService.Instance.GetConversationMessagesForConversationChannel(this.id)
        )
        .then(ConversationMessages => {
            this._conversationMessages = ConversationMessages ?? [];
            this._conversationMessagesSubject.next(this._conversationMessages);
            return this._conversationMessages;
         })
        .catch(err => {
            this._conversationMessages = [];
            this._conversationMessagesSubject.next(this._conversationMessages);
            throw err;
        })
        .finally(() => {
            this._conversationMessagesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessage. Call after mutations to force refresh.
     */
    public ClearConversationMessagesCache(): void {
        this._conversationMessages = null;
        this._conversationMessagesPromise = null;
        this._conversationMessagesSubject.next(this._conversationMessages);      // Emit to observable
    }

    public get HasConversationMessages(): Promise<boolean> {
        return this.ConversationMessages.then(conversationMessages => conversationMessages.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (conversationChannel.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await conversationChannel.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ConversationChannelData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ConversationChannelData>> {
        const info = await lastValueFrom(
            ConversationChannelService.Instance.GetConversationChannelChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ConversationChannelData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConversationChannelData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConversationChannelSubmitData {
        return ConversationChannelService.Instance.ConvertToConversationChannelSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConversationChannelService extends SecureEndpointBase {

    private static _instance: ConversationChannelService;
    private listCache: Map<string, Observable<Array<ConversationChannelData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConversationChannelBasicListData>>>;
    private recordCache: Map<string, Observable<ConversationChannelData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private conversationChannelChangeHistoryService: ConversationChannelChangeHistoryService,
        private conversationMessageService: ConversationMessageService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConversationChannelData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConversationChannelBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConversationChannelData>>();

        ConversationChannelService._instance = this;
    }

    public static get Instance(): ConversationChannelService {
      return ConversationChannelService._instance;
    }


    public ClearListCaches(config: ConversationChannelQueryParameters | null = null) {

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


    public ConvertToConversationChannelSubmitData(data: ConversationChannelData): ConversationChannelSubmitData {

        let output = new ConversationChannelSubmitData();

        output.id = data.id;
        output.conversationId = data.conversationId;
        output.name = data.name;
        output.topic = data.topic;
        output.isPrivate = data.isPrivate;
        output.isPinned = data.isPinned;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConversationChannel(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationChannelData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const conversationChannel$ = this.requestConversationChannel(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationChannel", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, conversationChannel$);

            return conversationChannel$;
        }

        return this.recordCache.get(configHash) as Observable<ConversationChannelData>;
    }

    private requestConversationChannel(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationChannelData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationChannelData>(this.baseUrl + 'api/ConversationChannel/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConversationChannel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationChannel(id, includeRelations));
            }));
    }

    public GetConversationChannelList(config: ConversationChannelQueryParameters | any = null) : Observable<Array<ConversationChannelData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const conversationChannelList$ = this.requestConversationChannelList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationChannel list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, conversationChannelList$);

            return conversationChannelList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConversationChannelData>>;
    }


    private requestConversationChannelList(config: ConversationChannelQueryParameters | any) : Observable <Array<ConversationChannelData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationChannelData>>(this.baseUrl + 'api/ConversationChannels', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConversationChannelList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationChannelList(config));
            }));
    }

    public GetConversationChannelsRowCount(config: ConversationChannelQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const conversationChannelsRowCount$ = this.requestConversationChannelsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationChannels row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, conversationChannelsRowCount$);

            return conversationChannelsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConversationChannelsRowCount(config: ConversationChannelQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConversationChannels/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationChannelsRowCount(config));
            }));
    }

    public GetConversationChannelsBasicListData(config: ConversationChannelQueryParameters | any = null) : Observable<Array<ConversationChannelBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const conversationChannelsBasicListData$ = this.requestConversationChannelsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationChannels basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, conversationChannelsBasicListData$);

            return conversationChannelsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConversationChannelBasicListData>>;
    }


    private requestConversationChannelsBasicListData(config: ConversationChannelQueryParameters | any) : Observable<Array<ConversationChannelBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationChannelBasicListData>>(this.baseUrl + 'api/ConversationChannels/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationChannelsBasicListData(config));
            }));

    }


    public PutConversationChannel(id: bigint | number, conversationChannel: ConversationChannelSubmitData) : Observable<ConversationChannelData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationChannelData>(this.baseUrl + 'api/ConversationChannel/' + id.toString(), conversationChannel, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationChannel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConversationChannel(id, conversationChannel));
            }));
    }


    public PostConversationChannel(conversationChannel: ConversationChannelSubmitData) : Observable<ConversationChannelData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConversationChannelData>(this.baseUrl + 'api/ConversationChannel', conversationChannel, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationChannel(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConversationChannel(conversationChannel));
            }));
    }

  
    public DeleteConversationChannel(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConversationChannel/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConversationChannel(id));
            }));
    }

    public RollbackConversationChannel(id: bigint | number, versionNumber: bigint | number) : Observable<ConversationChannelData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationChannelData>(this.baseUrl + 'api/ConversationChannel/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationChannel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackConversationChannel(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ConversationChannel.
     */
    public GetConversationChannelChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ConversationChannelData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationChannelData>>(this.baseUrl + 'api/ConversationChannel/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationChannelChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ConversationChannel.
     */
    public GetConversationChannelAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ConversationChannelData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationChannelData>[]>(this.baseUrl + 'api/ConversationChannel/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationChannelAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ConversationChannel.
     */
    public GetConversationChannelVersion(id: bigint | number, version: number): Observable<ConversationChannelData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationChannelData>(this.baseUrl + 'api/ConversationChannel/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationChannel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationChannelVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ConversationChannel at a specific point in time.
     */
    public GetConversationChannelStateAtTime(id: bigint | number, time: string): Observable<ConversationChannelData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationChannelData>(this.baseUrl + 'api/ConversationChannel/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationChannel(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationChannelStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ConversationChannelQueryParameters | any): string {

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

    public userIsSchedulerConversationChannelReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConversationChannelReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ConversationChannels
        //
        if (userIsSchedulerConversationChannelReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConversationChannelReader = user.readPermission >= 50;
            } else {
                userIsSchedulerConversationChannelReader = false;
            }
        }

        return userIsSchedulerConversationChannelReader;
    }


    public userIsSchedulerConversationChannelWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConversationChannelWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ConversationChannels
        //
        if (userIsSchedulerConversationChannelWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConversationChannelWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerConversationChannelWriter = false;
          }      
        }

        return userIsSchedulerConversationChannelWriter;
    }

    public GetConversationChannelChangeHistoriesForConversationChannel(conversationChannelId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationChannelChangeHistoryData[]> {
        return this.conversationChannelChangeHistoryService.GetConversationChannelChangeHistoryList({
            conversationChannelId: conversationChannelId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationMessagesForConversationChannel(conversationChannelId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageData[]> {
        return this.conversationMessageService.GetConversationMessageList({
            conversationChannelId: conversationChannelId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConversationChannelData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConversationChannelData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConversationChannelTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConversationChannel(raw: any): ConversationChannelData {
    if (!raw) return raw;

    //
    // Create a ConversationChannelData object instance with correct prototype
    //
    const revived = Object.create(ConversationChannelData.prototype) as ConversationChannelData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._conversationChannelChangeHistories = null;
    (revived as any)._conversationChannelChangeHistoriesPromise = null;
    (revived as any)._conversationChannelChangeHistoriesSubject = new BehaviorSubject<ConversationChannelChangeHistoryData[] | null>(null);

    (revived as any)._conversationMessages = null;
    (revived as any)._conversationMessagesPromise = null;
    (revived as any)._conversationMessagesSubject = new BehaviorSubject<ConversationMessageData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConversationChannelXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConversationChannelChangeHistories$ = (revived as any)._conversationChannelChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationChannelChangeHistories === null && (revived as any)._conversationChannelChangeHistoriesPromise === null) {
                (revived as any).loadConversationChannelChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationChannelChangeHistoriesCount$ = null;


    (revived as any).ConversationMessages$ = (revived as any)._conversationMessagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessages === null && (revived as any)._conversationMessagesPromise === null) {
                (revived as any).loadConversationMessages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessagesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationChannelData> | null>(null);

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

  private ReviveConversationChannelList(rawList: any[]): ConversationChannelData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConversationChannel(raw));
  }

}
