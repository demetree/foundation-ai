/*

   GENERATED SERVICE FOR THE CONVERSATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Conversation table.

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
import { ConversationTypeData } from './conversation-type.service';
import { ConversationUserService, ConversationUserData } from './conversation-user.service';
import { ConversationChannelService, ConversationChannelData } from './conversation-channel.service';
import { ConversationMessageService, ConversationMessageData } from './conversation-message.service';
import { ConversationPinService, ConversationPinData } from './conversation-pin.service';
import { ConversationThreadUserService, ConversationThreadUserData } from './conversation-thread-user.service';
import { CallService, CallData } from './call.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConversationQueryParameters {
    createdByUserId: bigint | number | null | undefined = null;
    conversationTypeId: bigint | number | null | undefined = null;
    priority: bigint | number | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
    entity: string | null | undefined = null;
    entityId: bigint | number | null | undefined = null;
    externalURL: string | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isPublic: boolean | null | undefined = null;
    userId: bigint | number | null | undefined = null;
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
export class ConversationSubmitData {
    id!: bigint | number;
    createdByUserId: bigint | number | null = null;
    conversationTypeId: bigint | number | null = null;
    priority!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    entity: string | null = null;
    entityId: bigint | number | null = null;
    externalURL: string | null = null;
    name: string | null = null;
    description: string | null = null;
    isPublic: boolean | null = null;
    userId: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ConversationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConversationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `conversation.ConversationChildren$` — use with `| async` in templates
//        • Promise:    `conversation.ConversationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="conversation.ConversationChildren$ | async"`), or
//        • Access the promise getter (`conversation.ConversationChildren` or `await conversation.ConversationChildren`)
//    - Simply reading `conversation.ConversationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await conversation.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConversationData {
    id!: bigint | number;
    createdByUserId!: bigint | number;
    conversationTypeId!: bigint | number;
    priority!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    entity!: string | null;
    entityId!: bigint | number;
    externalURL!: string | null;
    name!: string | null;
    description!: string | null;
    isPublic!: boolean | null;
    userId!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    conversationType: ConversationTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _conversationUsers: ConversationUserData[] | null = null;
    private _conversationUsersPromise: Promise<ConversationUserData[]> | null  = null;
    private _conversationUsersSubject = new BehaviorSubject<ConversationUserData[] | null>(null);

                
    private _conversationChannels: ConversationChannelData[] | null = null;
    private _conversationChannelsPromise: Promise<ConversationChannelData[]> | null  = null;
    private _conversationChannelsSubject = new BehaviorSubject<ConversationChannelData[] | null>(null);

                
    private _conversationMessages: ConversationMessageData[] | null = null;
    private _conversationMessagesPromise: Promise<ConversationMessageData[]> | null  = null;
    private _conversationMessagesSubject = new BehaviorSubject<ConversationMessageData[] | null>(null);

                
    private _conversationPins: ConversationPinData[] | null = null;
    private _conversationPinsPromise: Promise<ConversationPinData[]> | null  = null;
    private _conversationPinsSubject = new BehaviorSubject<ConversationPinData[] | null>(null);

                
    private _conversationThreadUsers: ConversationThreadUserData[] | null = null;
    private _conversationThreadUsersPromise: Promise<ConversationThreadUserData[]> | null  = null;
    private _conversationThreadUsersSubject = new BehaviorSubject<ConversationThreadUserData[] | null>(null);

                
    private _calls: CallData[] | null = null;
    private _callsPromise: Promise<CallData[]> | null  = null;
    private _callsSubject = new BehaviorSubject<CallData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConversationUsers$ = this._conversationUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationUsers === null && this._conversationUsersPromise === null) {
            this.loadConversationUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationUsersCount$: Observable<bigint | number> | null = null;
    public get ConversationUsersCount$(): Observable<bigint | number> {
        if (this._conversationUsersCount$ === null) {
            this._conversationUsersCount$ = ConversationUserService.Instance.GetConversationUsersRowCount({conversationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationUsersCount$;
    }



    public ConversationChannels$ = this._conversationChannelsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationChannels === null && this._conversationChannelsPromise === null) {
            this.loadConversationChannels(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationChannelsCount$: Observable<bigint | number> | null = null;
    public get ConversationChannelsCount$(): Observable<bigint | number> {
        if (this._conversationChannelsCount$ === null) {
            this._conversationChannelsCount$ = ConversationChannelService.Instance.GetConversationChannelsRowCount({conversationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationChannelsCount$;
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
            this._conversationMessagesCount$ = ConversationMessageService.Instance.GetConversationMessagesRowCount({conversationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessagesCount$;
    }



    public ConversationPins$ = this._conversationPinsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationPins === null && this._conversationPinsPromise === null) {
            this.loadConversationPins(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationPinsCount$: Observable<bigint | number> | null = null;
    public get ConversationPinsCount$(): Observable<bigint | number> {
        if (this._conversationPinsCount$ === null) {
            this._conversationPinsCount$ = ConversationPinService.Instance.GetConversationPinsRowCount({conversationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationPinsCount$;
    }



    public ConversationThreadUsers$ = this._conversationThreadUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationThreadUsers === null && this._conversationThreadUsersPromise === null) {
            this.loadConversationThreadUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationThreadUsersCount$: Observable<bigint | number> | null = null;
    public get ConversationThreadUsersCount$(): Observable<bigint | number> {
        if (this._conversationThreadUsersCount$ === null) {
            this._conversationThreadUsersCount$ = ConversationThreadUserService.Instance.GetConversationThreadUsersRowCount({conversationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationThreadUsersCount$;
    }



    public Calls$ = this._callsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._calls === null && this._callsPromise === null) {
            this.loadCalls(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _callsCount$: Observable<bigint | number> | null = null;
    public get CallsCount$(): Observable<bigint | number> {
        if (this._callsCount$ === null) {
            this._callsCount$ = CallService.Instance.GetCallsRowCount({conversationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._callsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConversationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.conversation.Reload();
  //
  //  Non Async:
  //
  //     conversation[0].Reload().then(x => {
  //        this.conversation = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConversationService.Instance.GetConversation(this.id, includeRelations)
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
     this._conversationUsers = null;
     this._conversationUsersPromise = null;
     this._conversationUsersSubject.next(null);
     this._conversationUsersCount$ = null;

     this._conversationChannels = null;
     this._conversationChannelsPromise = null;
     this._conversationChannelsSubject.next(null);
     this._conversationChannelsCount$ = null;

     this._conversationMessages = null;
     this._conversationMessagesPromise = null;
     this._conversationMessagesSubject.next(null);
     this._conversationMessagesCount$ = null;

     this._conversationPins = null;
     this._conversationPinsPromise = null;
     this._conversationPinsSubject.next(null);
     this._conversationPinsCount$ = null;

     this._conversationThreadUsers = null;
     this._conversationThreadUsersPromise = null;
     this._conversationThreadUsersSubject.next(null);
     this._conversationThreadUsersCount$ = null;

     this._calls = null;
     this._callsPromise = null;
     this._callsSubject.next(null);
     this._callsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ConversationUsers for this Conversation.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversation.ConversationUsers.then(conversations => { ... })
     *   or
     *   await this.conversation.conversations
     *
    */
    public get ConversationUsers(): Promise<ConversationUserData[]> {
        if (this._conversationUsers !== null) {
            return Promise.resolve(this._conversationUsers);
        }

        if (this._conversationUsersPromise !== null) {
            return this._conversationUsersPromise;
        }

        // Start the load
        this.loadConversationUsers();

        return this._conversationUsersPromise!;
    }



    private loadConversationUsers(): void {

        this._conversationUsersPromise = lastValueFrom(
            ConversationService.Instance.GetConversationUsersForConversation(this.id)
        )
        .then(ConversationUsers => {
            this._conversationUsers = ConversationUsers ?? [];
            this._conversationUsersSubject.next(this._conversationUsers);
            return this._conversationUsers;
         })
        .catch(err => {
            this._conversationUsers = [];
            this._conversationUsersSubject.next(this._conversationUsers);
            throw err;
        })
        .finally(() => {
            this._conversationUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationUser. Call after mutations to force refresh.
     */
    public ClearConversationUsersCache(): void {
        this._conversationUsers = null;
        this._conversationUsersPromise = null;
        this._conversationUsersSubject.next(this._conversationUsers);      // Emit to observable
    }

    public get HasConversationUsers(): Promise<boolean> {
        return this.ConversationUsers.then(conversationUsers => conversationUsers.length > 0);
    }


    /**
     *
     * Gets the ConversationChannels for this Conversation.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversation.ConversationChannels.then(conversations => { ... })
     *   or
     *   await this.conversation.conversations
     *
    */
    public get ConversationChannels(): Promise<ConversationChannelData[]> {
        if (this._conversationChannels !== null) {
            return Promise.resolve(this._conversationChannels);
        }

        if (this._conversationChannelsPromise !== null) {
            return this._conversationChannelsPromise;
        }

        // Start the load
        this.loadConversationChannels();

        return this._conversationChannelsPromise!;
    }



    private loadConversationChannels(): void {

        this._conversationChannelsPromise = lastValueFrom(
            ConversationService.Instance.GetConversationChannelsForConversation(this.id)
        )
        .then(ConversationChannels => {
            this._conversationChannels = ConversationChannels ?? [];
            this._conversationChannelsSubject.next(this._conversationChannels);
            return this._conversationChannels;
         })
        .catch(err => {
            this._conversationChannels = [];
            this._conversationChannelsSubject.next(this._conversationChannels);
            throw err;
        })
        .finally(() => {
            this._conversationChannelsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationChannel. Call after mutations to force refresh.
     */
    public ClearConversationChannelsCache(): void {
        this._conversationChannels = null;
        this._conversationChannelsPromise = null;
        this._conversationChannelsSubject.next(this._conversationChannels);      // Emit to observable
    }

    public get HasConversationChannels(): Promise<boolean> {
        return this.ConversationChannels.then(conversationChannels => conversationChannels.length > 0);
    }


    /**
     *
     * Gets the ConversationMessages for this Conversation.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversation.ConversationMessages.then(conversations => { ... })
     *   or
     *   await this.conversation.conversations
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
            ConversationService.Instance.GetConversationMessagesForConversation(this.id)
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


    /**
     *
     * Gets the ConversationPins for this Conversation.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversation.ConversationPins.then(conversations => { ... })
     *   or
     *   await this.conversation.conversations
     *
    */
    public get ConversationPins(): Promise<ConversationPinData[]> {
        if (this._conversationPins !== null) {
            return Promise.resolve(this._conversationPins);
        }

        if (this._conversationPinsPromise !== null) {
            return this._conversationPinsPromise;
        }

        // Start the load
        this.loadConversationPins();

        return this._conversationPinsPromise!;
    }



    private loadConversationPins(): void {

        this._conversationPinsPromise = lastValueFrom(
            ConversationService.Instance.GetConversationPinsForConversation(this.id)
        )
        .then(ConversationPins => {
            this._conversationPins = ConversationPins ?? [];
            this._conversationPinsSubject.next(this._conversationPins);
            return this._conversationPins;
         })
        .catch(err => {
            this._conversationPins = [];
            this._conversationPinsSubject.next(this._conversationPins);
            throw err;
        })
        .finally(() => {
            this._conversationPinsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationPin. Call after mutations to force refresh.
     */
    public ClearConversationPinsCache(): void {
        this._conversationPins = null;
        this._conversationPinsPromise = null;
        this._conversationPinsSubject.next(this._conversationPins);      // Emit to observable
    }

    public get HasConversationPins(): Promise<boolean> {
        return this.ConversationPins.then(conversationPins => conversationPins.length > 0);
    }


    /**
     *
     * Gets the ConversationThreadUsers for this Conversation.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversation.ConversationThreadUsers.then(conversations => { ... })
     *   or
     *   await this.conversation.conversations
     *
    */
    public get ConversationThreadUsers(): Promise<ConversationThreadUserData[]> {
        if (this._conversationThreadUsers !== null) {
            return Promise.resolve(this._conversationThreadUsers);
        }

        if (this._conversationThreadUsersPromise !== null) {
            return this._conversationThreadUsersPromise;
        }

        // Start the load
        this.loadConversationThreadUsers();

        return this._conversationThreadUsersPromise!;
    }



    private loadConversationThreadUsers(): void {

        this._conversationThreadUsersPromise = lastValueFrom(
            ConversationService.Instance.GetConversationThreadUsersForConversation(this.id)
        )
        .then(ConversationThreadUsers => {
            this._conversationThreadUsers = ConversationThreadUsers ?? [];
            this._conversationThreadUsersSubject.next(this._conversationThreadUsers);
            return this._conversationThreadUsers;
         })
        .catch(err => {
            this._conversationThreadUsers = [];
            this._conversationThreadUsersSubject.next(this._conversationThreadUsers);
            throw err;
        })
        .finally(() => {
            this._conversationThreadUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationThreadUser. Call after mutations to force refresh.
     */
    public ClearConversationThreadUsersCache(): void {
        this._conversationThreadUsers = null;
        this._conversationThreadUsersPromise = null;
        this._conversationThreadUsersSubject.next(this._conversationThreadUsers);      // Emit to observable
    }

    public get HasConversationThreadUsers(): Promise<boolean> {
        return this.ConversationThreadUsers.then(conversationThreadUsers => conversationThreadUsers.length > 0);
    }


    /**
     *
     * Gets the Calls for this Conversation.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversation.Calls.then(conversations => { ... })
     *   or
     *   await this.conversation.conversations
     *
    */
    public get Calls(): Promise<CallData[]> {
        if (this._calls !== null) {
            return Promise.resolve(this._calls);
        }

        if (this._callsPromise !== null) {
            return this._callsPromise;
        }

        // Start the load
        this.loadCalls();

        return this._callsPromise!;
    }



    private loadCalls(): void {

        this._callsPromise = lastValueFrom(
            ConversationService.Instance.GetCallsForConversation(this.id)
        )
        .then(Calls => {
            this._calls = Calls ?? [];
            this._callsSubject.next(this._calls);
            return this._calls;
         })
        .catch(err => {
            this._calls = [];
            this._callsSubject.next(this._calls);
            throw err;
        })
        .finally(() => {
            this._callsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Call. Call after mutations to force refresh.
     */
    public ClearCallsCache(): void {
        this._calls = null;
        this._callsPromise = null;
        this._callsSubject.next(this._calls);      // Emit to observable
    }

    public get HasCalls(): Promise<boolean> {
        return this.Calls.then(calls => calls.length > 0);
    }




    /**
     * Updates the state of this ConversationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConversationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConversationSubmitData {
        return ConversationService.Instance.ConvertToConversationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConversationService extends SecureEndpointBase {

    private static _instance: ConversationService;
    private listCache: Map<string, Observable<Array<ConversationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConversationBasicListData>>>;
    private recordCache: Map<string, Observable<ConversationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private conversationUserService: ConversationUserService,
        private conversationChannelService: ConversationChannelService,
        private conversationMessageService: ConversationMessageService,
        private conversationPinService: ConversationPinService,
        private conversationThreadUserService: ConversationThreadUserService,
        private callService: CallService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConversationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConversationBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConversationData>>();

        ConversationService._instance = this;
    }

    public static get Instance(): ConversationService {
      return ConversationService._instance;
    }


    public ClearListCaches(config: ConversationQueryParameters | null = null) {

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


    public ConvertToConversationSubmitData(data: ConversationData): ConversationSubmitData {

        let output = new ConversationSubmitData();

        output.id = data.id;
        output.createdByUserId = data.createdByUserId;
        output.conversationTypeId = data.conversationTypeId;
        output.priority = data.priority;
        output.dateTimeCreated = data.dateTimeCreated;
        output.entity = data.entity;
        output.entityId = data.entityId;
        output.externalURL = data.externalURL;
        output.name = data.name;
        output.description = data.description;
        output.isPublic = data.isPublic;
        output.userId = data.userId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConversation(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const conversation$ = this.requestConversation(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Conversation", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, conversation$);

            return conversation$;
        }

        return this.recordCache.get(configHash) as Observable<ConversationData>;
    }

    private requestConversation(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationData>(this.baseUrl + 'api/Conversation/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConversation(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversation(id, includeRelations));
            }));
    }

    public GetConversationList(config: ConversationQueryParameters | any = null) : Observable<Array<ConversationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const conversationList$ = this.requestConversationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Conversation list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, conversationList$);

            return conversationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConversationData>>;
    }


    private requestConversationList(config: ConversationQueryParameters | any) : Observable <Array<ConversationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationData>>(this.baseUrl + 'api/Conversations', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConversationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationList(config));
            }));
    }

    public GetConversationsRowCount(config: ConversationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const conversationsRowCount$ = this.requestConversationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Conversations row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, conversationsRowCount$);

            return conversationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConversationsRowCount(config: ConversationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Conversations/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationsRowCount(config));
            }));
    }

    public GetConversationsBasicListData(config: ConversationQueryParameters | any = null) : Observable<Array<ConversationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const conversationsBasicListData$ = this.requestConversationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Conversations basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, conversationsBasicListData$);

            return conversationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConversationBasicListData>>;
    }


    private requestConversationsBasicListData(config: ConversationQueryParameters | any) : Observable<Array<ConversationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationBasicListData>>(this.baseUrl + 'api/Conversations/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationsBasicListData(config));
            }));

    }


    public PutConversation(id: bigint | number, conversation: ConversationSubmitData) : Observable<ConversationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationData>(this.baseUrl + 'api/Conversation/' + id.toString(), conversation, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversation(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConversation(id, conversation));
            }));
    }


    public PostConversation(conversation: ConversationSubmitData) : Observable<ConversationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConversationData>(this.baseUrl + 'api/Conversation', conversation, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversation(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConversation(conversation));
            }));
    }

  
    public DeleteConversation(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Conversation/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConversation(id));
            }));
    }


    private getConfigHash(config: ConversationQueryParameters | any): string {

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

    public userIsSchedulerConversationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConversationReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Conversations
        //
        if (userIsSchedulerConversationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConversationReader = user.readPermission >= 50;
            } else {
                userIsSchedulerConversationReader = false;
            }
        }

        return userIsSchedulerConversationReader;
    }


    public userIsSchedulerConversationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConversationWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Conversations
        //
        if (userIsSchedulerConversationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConversationWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerConversationWriter = false;
          }      
        }

        return userIsSchedulerConversationWriter;
    }

    public GetConversationUsersForConversation(conversationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationUserData[]> {
        return this.conversationUserService.GetConversationUserList({
            conversationId: conversationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationChannelsForConversation(conversationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationChannelData[]> {
        return this.conversationChannelService.GetConversationChannelList({
            conversationId: conversationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationMessagesForConversation(conversationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageData[]> {
        return this.conversationMessageService.GetConversationMessageList({
            conversationId: conversationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationPinsForConversation(conversationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationPinData[]> {
        return this.conversationPinService.GetConversationPinList({
            conversationId: conversationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationThreadUsersForConversation(conversationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationThreadUserData[]> {
        return this.conversationThreadUserService.GetConversationThreadUserList({
            conversationId: conversationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCallsForConversation(conversationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CallData[]> {
        return this.callService.GetCallList({
            conversationId: conversationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConversationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConversationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConversationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConversation(raw: any): ConversationData {
    if (!raw) return raw;

    //
    // Create a ConversationData object instance with correct prototype
    //
    const revived = Object.create(ConversationData.prototype) as ConversationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._conversationUsers = null;
    (revived as any)._conversationUsersPromise = null;
    (revived as any)._conversationUsersSubject = new BehaviorSubject<ConversationUserData[] | null>(null);

    (revived as any)._conversationChannels = null;
    (revived as any)._conversationChannelsPromise = null;
    (revived as any)._conversationChannelsSubject = new BehaviorSubject<ConversationChannelData[] | null>(null);

    (revived as any)._conversationMessages = null;
    (revived as any)._conversationMessagesPromise = null;
    (revived as any)._conversationMessagesSubject = new BehaviorSubject<ConversationMessageData[] | null>(null);

    (revived as any)._conversationPins = null;
    (revived as any)._conversationPinsPromise = null;
    (revived as any)._conversationPinsSubject = new BehaviorSubject<ConversationPinData[] | null>(null);

    (revived as any)._conversationThreadUsers = null;
    (revived as any)._conversationThreadUsersPromise = null;
    (revived as any)._conversationThreadUsersSubject = new BehaviorSubject<ConversationThreadUserData[] | null>(null);

    (revived as any)._calls = null;
    (revived as any)._callsPromise = null;
    (revived as any)._callsSubject = new BehaviorSubject<CallData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConversationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConversationUsers$ = (revived as any)._conversationUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationUsers === null && (revived as any)._conversationUsersPromise === null) {
                (revived as any).loadConversationUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationUsersCount$ = null;


    (revived as any).ConversationChannels$ = (revived as any)._conversationChannelsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationChannels === null && (revived as any)._conversationChannelsPromise === null) {
                (revived as any).loadConversationChannels();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationChannelsCount$ = null;


    (revived as any).ConversationMessages$ = (revived as any)._conversationMessagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessages === null && (revived as any)._conversationMessagesPromise === null) {
                (revived as any).loadConversationMessages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessagesCount$ = null;


    (revived as any).ConversationPins$ = (revived as any)._conversationPinsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationPins === null && (revived as any)._conversationPinsPromise === null) {
                (revived as any).loadConversationPins();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationPinsCount$ = null;


    (revived as any).ConversationThreadUsers$ = (revived as any)._conversationThreadUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationThreadUsers === null && (revived as any)._conversationThreadUsersPromise === null) {
                (revived as any).loadConversationThreadUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationThreadUsersCount$ = null;


    (revived as any).Calls$ = (revived as any)._callsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._calls === null && (revived as any)._callsPromise === null) {
                (revived as any).loadCalls();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._callsCount$ = null;



    return revived;
  }

  private ReviveConversationList(rawList: any[]): ConversationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConversation(raw));
  }

}
