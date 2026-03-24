/*

   GENERATED SERVICE FOR THE CONVERSATIONMESSAGE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ConversationMessage table.

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
import { ConversationChannelData } from './conversation-channel.service';
import { ConversationMessageChangeHistoryService, ConversationMessageChangeHistoryData } from './conversation-message-change-history.service';
import { ConversationMessageAttachmentService, ConversationMessageAttachmentData } from './conversation-message-attachment.service';
import { ConversationMessageUserService, ConversationMessageUserData } from './conversation-message-user.service';
import { ConversationMessageReactionService, ConversationMessageReactionData } from './conversation-message-reaction.service';
import { ConversationPinService, ConversationPinData } from './conversation-pin.service';
import { ConversationMessageLinkPreviewService, ConversationMessageLinkPreviewData } from './conversation-message-link-preview.service';
import { ConversationThreadUserService, ConversationThreadUserData } from './conversation-thread-user.service';
import { MessageBookmarkService, MessageBookmarkData } from './message-bookmark.service';
import { MessageFlagService, MessageFlagData } from './message-flag.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ConversationMessageQueryParameters {
    conversationId: bigint | number | null | undefined = null;
    userId: bigint | number | null | undefined = null;
    parentConversationMessageId: bigint | number | null | undefined = null;
    conversationChannelId: bigint | number | null | undefined = null;
    dateTimeCreated: string | null | undefined = null;        // ISO 8601 (full datetime)
    message: string | null | undefined = null;
    messageType: string | null | undefined = null;
    entity: string | null | undefined = null;
    entityId: bigint | number | null | undefined = null;
    externalURL: string | null | undefined = null;
    forwardedFromMessageId: bigint | number | null | undefined = null;
    forwardedFromUserId: bigint | number | null | undefined = null;
    isScheduled: boolean | null | undefined = null;
    scheduledDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class ConversationMessageSubmitData {
    id!: bigint | number;
    conversationId!: bigint | number;
    userId!: bigint | number;
    parentConversationMessageId: bigint | number | null = null;
    conversationChannelId: bigint | number | null = null;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    message!: string;
    messageType: string | null = null;
    entity: string | null = null;
    entityId: bigint | number | null = null;
    externalURL: string | null = null;
    forwardedFromMessageId: bigint | number | null = null;
    forwardedFromUserId: bigint | number | null = null;
    isScheduled!: boolean;
    scheduledDateTime: string | null = null;     // ISO 8601 (full datetime)
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

export class ConversationMessageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ConversationMessageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `conversationMessage.ConversationMessageChildren$` — use with `| async` in templates
//        • Promise:    `conversationMessage.ConversationMessageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="conversationMessage.ConversationMessageChildren$ | async"`), or
//        • Access the promise getter (`conversationMessage.ConversationMessageChildren` or `await conversationMessage.ConversationMessageChildren`)
//    - Simply reading `conversationMessage.ConversationMessageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await conversationMessage.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ConversationMessageData {
    id!: bigint | number;
    conversationId!: bigint | number;
    userId!: bigint | number;
    parentConversationMessageId!: bigint | number;
    conversationChannelId!: bigint | number;
    dateTimeCreated!: string;      // ISO 8601 (full datetime)
    message!: string;
    messageType!: string | null;
    entity!: string | null;
    entityId!: bigint | number;
    externalURL!: string | null;
    forwardedFromMessageId!: bigint | number;
    forwardedFromUserId!: bigint | number;
    isScheduled!: boolean;
    scheduledDateTime!: string | null;   // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    conversation: ConversationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    conversationChannel: ConversationChannelData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    parentConversationMessage: ConversationMessageData | null | undefined = null;            // Self referencing navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _conversationMessageChangeHistories: ConversationMessageChangeHistoryData[] | null = null;
    private _conversationMessageChangeHistoriesPromise: Promise<ConversationMessageChangeHistoryData[]> | null  = null;
    private _conversationMessageChangeHistoriesSubject = new BehaviorSubject<ConversationMessageChangeHistoryData[] | null>(null);

                
    private _conversationMessageAttachments: ConversationMessageAttachmentData[] | null = null;
    private _conversationMessageAttachmentsPromise: Promise<ConversationMessageAttachmentData[]> | null  = null;
    private _conversationMessageAttachmentsSubject = new BehaviorSubject<ConversationMessageAttachmentData[] | null>(null);

                
    private _conversationMessageUsers: ConversationMessageUserData[] | null = null;
    private _conversationMessageUsersPromise: Promise<ConversationMessageUserData[]> | null  = null;
    private _conversationMessageUsersSubject = new BehaviorSubject<ConversationMessageUserData[] | null>(null);

                
    private _conversationMessageReactions: ConversationMessageReactionData[] | null = null;
    private _conversationMessageReactionsPromise: Promise<ConversationMessageReactionData[]> | null  = null;
    private _conversationMessageReactionsSubject = new BehaviorSubject<ConversationMessageReactionData[] | null>(null);

                
    private _conversationPins: ConversationPinData[] | null = null;
    private _conversationPinsPromise: Promise<ConversationPinData[]> | null  = null;
    private _conversationPinsSubject = new BehaviorSubject<ConversationPinData[] | null>(null);

                
    private _conversationMessageLinkPreviews: ConversationMessageLinkPreviewData[] | null = null;
    private _conversationMessageLinkPreviewsPromise: Promise<ConversationMessageLinkPreviewData[]> | null  = null;
    private _conversationMessageLinkPreviewsSubject = new BehaviorSubject<ConversationMessageLinkPreviewData[] | null>(null);

                
    private _conversationThreadUserParentConversationMessages: ConversationThreadUserData[] | null = null;
    private _conversationThreadUserParentConversationMessagesPromise: Promise<ConversationThreadUserData[]> | null  = null;
    private _conversationThreadUserParentConversationMessagesSubject = new BehaviorSubject<ConversationThreadUserData[] | null>(null);
                    
    private _messageBookmarks: MessageBookmarkData[] | null = null;
    private _messageBookmarksPromise: Promise<MessageBookmarkData[]> | null  = null;
    private _messageBookmarksSubject = new BehaviorSubject<MessageBookmarkData[] | null>(null);

                
    private _messageFlags: MessageFlagData[] | null = null;
    private _messageFlagsPromise: Promise<MessageFlagData[]> | null  = null;
    private _messageFlagsSubject = new BehaviorSubject<MessageFlagData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ConversationMessageData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ConversationMessageData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationMessageData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ConversationMessageChangeHistories$ = this._conversationMessageChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessageChangeHistories === null && this._conversationMessageChangeHistoriesPromise === null) {
            this.loadConversationMessageChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessageChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ConversationMessageChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._conversationMessageChangeHistoriesCount$ === null) {
            this._conversationMessageChangeHistoriesCount$ = ConversationMessageChangeHistoryService.Instance.GetConversationMessageChangeHistoriesRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessageChangeHistoriesCount$;
    }



    public ConversationMessageAttachments$ = this._conversationMessageAttachmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessageAttachments === null && this._conversationMessageAttachmentsPromise === null) {
            this.loadConversationMessageAttachments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessageAttachmentsCount$: Observable<bigint | number> | null = null;
    public get ConversationMessageAttachmentsCount$(): Observable<bigint | number> {
        if (this._conversationMessageAttachmentsCount$ === null) {
            this._conversationMessageAttachmentsCount$ = ConversationMessageAttachmentService.Instance.GetConversationMessageAttachmentsRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessageAttachmentsCount$;
    }



    public ConversationMessageUsers$ = this._conversationMessageUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessageUsers === null && this._conversationMessageUsersPromise === null) {
            this.loadConversationMessageUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessageUsersCount$: Observable<bigint | number> | null = null;
    public get ConversationMessageUsersCount$(): Observable<bigint | number> {
        if (this._conversationMessageUsersCount$ === null) {
            this._conversationMessageUsersCount$ = ConversationMessageUserService.Instance.GetConversationMessageUsersRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessageUsersCount$;
    }



    public ConversationMessageReactions$ = this._conversationMessageReactionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessageReactions === null && this._conversationMessageReactionsPromise === null) {
            this.loadConversationMessageReactions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessageReactionsCount$: Observable<bigint | number> | null = null;
    public get ConversationMessageReactionsCount$(): Observable<bigint | number> {
        if (this._conversationMessageReactionsCount$ === null) {
            this._conversationMessageReactionsCount$ = ConversationMessageReactionService.Instance.GetConversationMessageReactionsRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessageReactionsCount$;
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
            this._conversationPinsCount$ = ConversationPinService.Instance.GetConversationPinsRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationPinsCount$;
    }



    public ConversationMessageLinkPreviews$ = this._conversationMessageLinkPreviewsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationMessageLinkPreviews === null && this._conversationMessageLinkPreviewsPromise === null) {
            this.loadConversationMessageLinkPreviews(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationMessageLinkPreviewsCount$: Observable<bigint | number> | null = null;
    public get ConversationMessageLinkPreviewsCount$(): Observable<bigint | number> {
        if (this._conversationMessageLinkPreviewsCount$ === null) {
            this._conversationMessageLinkPreviewsCount$ = ConversationMessageLinkPreviewService.Instance.GetConversationMessageLinkPreviewsRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationMessageLinkPreviewsCount$;
    }



    public ConversationThreadUserParentConversationMessages$ = this._conversationThreadUserParentConversationMessagesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._conversationThreadUserParentConversationMessages === null && this._conversationThreadUserParentConversationMessagesPromise === null) {
            this.loadConversationThreadUserParentConversationMessages(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _conversationThreadUserParentConversationMessagesCount$: Observable<bigint | number> | null = null;
    public get ConversationThreadUserParentConversationMessagesCount$(): Observable<bigint | number> {
        if (this._conversationThreadUserParentConversationMessagesCount$ === null) {
            this._conversationThreadUserParentConversationMessagesCount$ = ConversationThreadUserService.Instance.GetConversationThreadUsersRowCount({parentConversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._conversationThreadUserParentConversationMessagesCount$;
    }


    public MessageBookmarks$ = this._messageBookmarksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._messageBookmarks === null && this._messageBookmarksPromise === null) {
            this.loadMessageBookmarks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _messageBookmarksCount$: Observable<bigint | number> | null = null;
    public get MessageBookmarksCount$(): Observable<bigint | number> {
        if (this._messageBookmarksCount$ === null) {
            this._messageBookmarksCount$ = MessageBookmarkService.Instance.GetMessageBookmarksRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._messageBookmarksCount$;
    }



    public MessageFlags$ = this._messageFlagsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._messageFlags === null && this._messageFlagsPromise === null) {
            this.loadMessageFlags(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _messageFlagsCount$: Observable<bigint | number> | null = null;
    public get MessageFlagsCount$(): Observable<bigint | number> {
        if (this._messageFlagsCount$ === null) {
            this._messageFlagsCount$ = MessageFlagService.Instance.GetMessageFlagsRowCount({conversationMessageId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._messageFlagsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ConversationMessageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.conversationMessage.Reload();
  //
  //  Non Async:
  //
  //     conversationMessage[0].Reload().then(x => {
  //        this.conversationMessage = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ConversationMessageService.Instance.GetConversationMessage(this.id, includeRelations)
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
     this._conversationMessageChangeHistories = null;
     this._conversationMessageChangeHistoriesPromise = null;
     this._conversationMessageChangeHistoriesSubject.next(null);
     this._conversationMessageChangeHistoriesCount$ = null;

     this._conversationMessageAttachments = null;
     this._conversationMessageAttachmentsPromise = null;
     this._conversationMessageAttachmentsSubject.next(null);
     this._conversationMessageAttachmentsCount$ = null;

     this._conversationMessageUsers = null;
     this._conversationMessageUsersPromise = null;
     this._conversationMessageUsersSubject.next(null);
     this._conversationMessageUsersCount$ = null;

     this._conversationMessageReactions = null;
     this._conversationMessageReactionsPromise = null;
     this._conversationMessageReactionsSubject.next(null);
     this._conversationMessageReactionsCount$ = null;

     this._conversationPins = null;
     this._conversationPinsPromise = null;
     this._conversationPinsSubject.next(null);
     this._conversationPinsCount$ = null;

     this._conversationMessageLinkPreviews = null;
     this._conversationMessageLinkPreviewsPromise = null;
     this._conversationMessageLinkPreviewsSubject.next(null);
     this._conversationMessageLinkPreviewsCount$ = null;

     this._conversationThreadUserParentConversationMessages = null;
     this._conversationThreadUserParentConversationMessagesPromise = null;
     this._conversationThreadUserParentConversationMessagesSubject.next(null);
     this._conversationThreadUserParentConversationMessagesCount$ = null;

     this._messageBookmarks = null;
     this._messageBookmarksPromise = null;
     this._messageBookmarksSubject.next(null);
     this._messageBookmarksCount$ = null;

     this._messageFlags = null;
     this._messageFlagsPromise = null;
     this._messageFlagsSubject.next(null);
     this._messageFlagsCount$ = null;

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
     * Gets the ConversationMessageChangeHistories for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.ConversationMessageChangeHistories.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
     *
    */
    public get ConversationMessageChangeHistories(): Promise<ConversationMessageChangeHistoryData[]> {
        if (this._conversationMessageChangeHistories !== null) {
            return Promise.resolve(this._conversationMessageChangeHistories);
        }

        if (this._conversationMessageChangeHistoriesPromise !== null) {
            return this._conversationMessageChangeHistoriesPromise;
        }

        // Start the load
        this.loadConversationMessageChangeHistories();

        return this._conversationMessageChangeHistoriesPromise!;
    }



    private loadConversationMessageChangeHistories(): void {

        this._conversationMessageChangeHistoriesPromise = lastValueFrom(
            ConversationMessageService.Instance.GetConversationMessageChangeHistoriesForConversationMessage(this.id)
        )
        .then(ConversationMessageChangeHistories => {
            this._conversationMessageChangeHistories = ConversationMessageChangeHistories ?? [];
            this._conversationMessageChangeHistoriesSubject.next(this._conversationMessageChangeHistories);
            return this._conversationMessageChangeHistories;
         })
        .catch(err => {
            this._conversationMessageChangeHistories = [];
            this._conversationMessageChangeHistoriesSubject.next(this._conversationMessageChangeHistories);
            throw err;
        })
        .finally(() => {
            this._conversationMessageChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessageChangeHistory. Call after mutations to force refresh.
     */
    public ClearConversationMessageChangeHistoriesCache(): void {
        this._conversationMessageChangeHistories = null;
        this._conversationMessageChangeHistoriesPromise = null;
        this._conversationMessageChangeHistoriesSubject.next(this._conversationMessageChangeHistories);      // Emit to observable
    }

    public get HasConversationMessageChangeHistories(): Promise<boolean> {
        return this.ConversationMessageChangeHistories.then(conversationMessageChangeHistories => conversationMessageChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ConversationMessageAttachments for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.ConversationMessageAttachments.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
     *
    */
    public get ConversationMessageAttachments(): Promise<ConversationMessageAttachmentData[]> {
        if (this._conversationMessageAttachments !== null) {
            return Promise.resolve(this._conversationMessageAttachments);
        }

        if (this._conversationMessageAttachmentsPromise !== null) {
            return this._conversationMessageAttachmentsPromise;
        }

        // Start the load
        this.loadConversationMessageAttachments();

        return this._conversationMessageAttachmentsPromise!;
    }



    private loadConversationMessageAttachments(): void {

        this._conversationMessageAttachmentsPromise = lastValueFrom(
            ConversationMessageService.Instance.GetConversationMessageAttachmentsForConversationMessage(this.id)
        )
        .then(ConversationMessageAttachments => {
            this._conversationMessageAttachments = ConversationMessageAttachments ?? [];
            this._conversationMessageAttachmentsSubject.next(this._conversationMessageAttachments);
            return this._conversationMessageAttachments;
         })
        .catch(err => {
            this._conversationMessageAttachments = [];
            this._conversationMessageAttachmentsSubject.next(this._conversationMessageAttachments);
            throw err;
        })
        .finally(() => {
            this._conversationMessageAttachmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessageAttachment. Call after mutations to force refresh.
     */
    public ClearConversationMessageAttachmentsCache(): void {
        this._conversationMessageAttachments = null;
        this._conversationMessageAttachmentsPromise = null;
        this._conversationMessageAttachmentsSubject.next(this._conversationMessageAttachments);      // Emit to observable
    }

    public get HasConversationMessageAttachments(): Promise<boolean> {
        return this.ConversationMessageAttachments.then(conversationMessageAttachments => conversationMessageAttachments.length > 0);
    }


    /**
     *
     * Gets the ConversationMessageUsers for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.ConversationMessageUsers.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
     *
    */
    public get ConversationMessageUsers(): Promise<ConversationMessageUserData[]> {
        if (this._conversationMessageUsers !== null) {
            return Promise.resolve(this._conversationMessageUsers);
        }

        if (this._conversationMessageUsersPromise !== null) {
            return this._conversationMessageUsersPromise;
        }

        // Start the load
        this.loadConversationMessageUsers();

        return this._conversationMessageUsersPromise!;
    }



    private loadConversationMessageUsers(): void {

        this._conversationMessageUsersPromise = lastValueFrom(
            ConversationMessageService.Instance.GetConversationMessageUsersForConversationMessage(this.id)
        )
        .then(ConversationMessageUsers => {
            this._conversationMessageUsers = ConversationMessageUsers ?? [];
            this._conversationMessageUsersSubject.next(this._conversationMessageUsers);
            return this._conversationMessageUsers;
         })
        .catch(err => {
            this._conversationMessageUsers = [];
            this._conversationMessageUsersSubject.next(this._conversationMessageUsers);
            throw err;
        })
        .finally(() => {
            this._conversationMessageUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessageUser. Call after mutations to force refresh.
     */
    public ClearConversationMessageUsersCache(): void {
        this._conversationMessageUsers = null;
        this._conversationMessageUsersPromise = null;
        this._conversationMessageUsersSubject.next(this._conversationMessageUsers);      // Emit to observable
    }

    public get HasConversationMessageUsers(): Promise<boolean> {
        return this.ConversationMessageUsers.then(conversationMessageUsers => conversationMessageUsers.length > 0);
    }


    /**
     *
     * Gets the ConversationMessageReactions for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.ConversationMessageReactions.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
     *
    */
    public get ConversationMessageReactions(): Promise<ConversationMessageReactionData[]> {
        if (this._conversationMessageReactions !== null) {
            return Promise.resolve(this._conversationMessageReactions);
        }

        if (this._conversationMessageReactionsPromise !== null) {
            return this._conversationMessageReactionsPromise;
        }

        // Start the load
        this.loadConversationMessageReactions();

        return this._conversationMessageReactionsPromise!;
    }



    private loadConversationMessageReactions(): void {

        this._conversationMessageReactionsPromise = lastValueFrom(
            ConversationMessageService.Instance.GetConversationMessageReactionsForConversationMessage(this.id)
        )
        .then(ConversationMessageReactions => {
            this._conversationMessageReactions = ConversationMessageReactions ?? [];
            this._conversationMessageReactionsSubject.next(this._conversationMessageReactions);
            return this._conversationMessageReactions;
         })
        .catch(err => {
            this._conversationMessageReactions = [];
            this._conversationMessageReactionsSubject.next(this._conversationMessageReactions);
            throw err;
        })
        .finally(() => {
            this._conversationMessageReactionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessageReaction. Call after mutations to force refresh.
     */
    public ClearConversationMessageReactionsCache(): void {
        this._conversationMessageReactions = null;
        this._conversationMessageReactionsPromise = null;
        this._conversationMessageReactionsSubject.next(this._conversationMessageReactions);      // Emit to observable
    }

    public get HasConversationMessageReactions(): Promise<boolean> {
        return this.ConversationMessageReactions.then(conversationMessageReactions => conversationMessageReactions.length > 0);
    }


    /**
     *
     * Gets the ConversationPins for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.ConversationPins.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
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
            ConversationMessageService.Instance.GetConversationPinsForConversationMessage(this.id)
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
     * Gets the ConversationMessageLinkPreviews for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.ConversationMessageLinkPreviews.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
     *
    */
    public get ConversationMessageLinkPreviews(): Promise<ConversationMessageLinkPreviewData[]> {
        if (this._conversationMessageLinkPreviews !== null) {
            return Promise.resolve(this._conversationMessageLinkPreviews);
        }

        if (this._conversationMessageLinkPreviewsPromise !== null) {
            return this._conversationMessageLinkPreviewsPromise;
        }

        // Start the load
        this.loadConversationMessageLinkPreviews();

        return this._conversationMessageLinkPreviewsPromise!;
    }



    private loadConversationMessageLinkPreviews(): void {

        this._conversationMessageLinkPreviewsPromise = lastValueFrom(
            ConversationMessageService.Instance.GetConversationMessageLinkPreviewsForConversationMessage(this.id)
        )
        .then(ConversationMessageLinkPreviews => {
            this._conversationMessageLinkPreviews = ConversationMessageLinkPreviews ?? [];
            this._conversationMessageLinkPreviewsSubject.next(this._conversationMessageLinkPreviews);
            return this._conversationMessageLinkPreviews;
         })
        .catch(err => {
            this._conversationMessageLinkPreviews = [];
            this._conversationMessageLinkPreviewsSubject.next(this._conversationMessageLinkPreviews);
            throw err;
        })
        .finally(() => {
            this._conversationMessageLinkPreviewsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationMessageLinkPreview. Call after mutations to force refresh.
     */
    public ClearConversationMessageLinkPreviewsCache(): void {
        this._conversationMessageLinkPreviews = null;
        this._conversationMessageLinkPreviewsPromise = null;
        this._conversationMessageLinkPreviewsSubject.next(this._conversationMessageLinkPreviews);      // Emit to observable
    }

    public get HasConversationMessageLinkPreviews(): Promise<boolean> {
        return this.ConversationMessageLinkPreviews.then(conversationMessageLinkPreviews => conversationMessageLinkPreviews.length > 0);
    }


    /**
     *
     * Gets the ConversationThreadUserParentConversationMessages for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.ConversationThreadUserParentConversationMessages.then(parentConversationMessages => { ... })
     *   or
     *   await this.conversationMessage.parentConversationMessages
     *
    */
    public get ConversationThreadUserParentConversationMessages(): Promise<ConversationThreadUserData[]> {
        if (this._conversationThreadUserParentConversationMessages !== null) {
            return Promise.resolve(this._conversationThreadUserParentConversationMessages);
        }

        if (this._conversationThreadUserParentConversationMessagesPromise !== null) {
            return this._conversationThreadUserParentConversationMessagesPromise;
        }

        // Start the load
        this.loadConversationThreadUserParentConversationMessages();

        return this._conversationThreadUserParentConversationMessagesPromise!;
    }



    private loadConversationThreadUserParentConversationMessages(): void {

        this._conversationThreadUserParentConversationMessagesPromise = lastValueFrom(
            ConversationMessageService.Instance.GetConversationThreadUserParentConversationMessagesForConversationMessage(this.id)
        )
        .then(ConversationThreadUserParentConversationMessages => {
            this._conversationThreadUserParentConversationMessages = ConversationThreadUserParentConversationMessages ?? [];
            this._conversationThreadUserParentConversationMessagesSubject.next(this._conversationThreadUserParentConversationMessages);
            return this._conversationThreadUserParentConversationMessages;
         })
        .catch(err => {
            this._conversationThreadUserParentConversationMessages = [];
            this._conversationThreadUserParentConversationMessagesSubject.next(this._conversationThreadUserParentConversationMessages);
            throw err;
        })
        .finally(() => {
            this._conversationThreadUserParentConversationMessagesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConversationThreadUserParentConversationMessage. Call after mutations to force refresh.
     */
    public ClearConversationThreadUserParentConversationMessagesCache(): void {
        this._conversationThreadUserParentConversationMessages = null;
        this._conversationThreadUserParentConversationMessagesPromise = null;
        this._conversationThreadUserParentConversationMessagesSubject.next(this._conversationThreadUserParentConversationMessages);      // Emit to observable
    }

    public get HasConversationThreadUserParentConversationMessages(): Promise<boolean> {
        return this.ConversationThreadUserParentConversationMessages.then(conversationThreadUserParentConversationMessages => conversationThreadUserParentConversationMessages.length > 0);
    }


    /**
     *
     * Gets the MessageBookmarks for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.MessageBookmarks.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
     *
    */
    public get MessageBookmarks(): Promise<MessageBookmarkData[]> {
        if (this._messageBookmarks !== null) {
            return Promise.resolve(this._messageBookmarks);
        }

        if (this._messageBookmarksPromise !== null) {
            return this._messageBookmarksPromise;
        }

        // Start the load
        this.loadMessageBookmarks();

        return this._messageBookmarksPromise!;
    }



    private loadMessageBookmarks(): void {

        this._messageBookmarksPromise = lastValueFrom(
            ConversationMessageService.Instance.GetMessageBookmarksForConversationMessage(this.id)
        )
        .then(MessageBookmarks => {
            this._messageBookmarks = MessageBookmarks ?? [];
            this._messageBookmarksSubject.next(this._messageBookmarks);
            return this._messageBookmarks;
         })
        .catch(err => {
            this._messageBookmarks = [];
            this._messageBookmarksSubject.next(this._messageBookmarks);
            throw err;
        })
        .finally(() => {
            this._messageBookmarksPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MessageBookmark. Call after mutations to force refresh.
     */
    public ClearMessageBookmarksCache(): void {
        this._messageBookmarks = null;
        this._messageBookmarksPromise = null;
        this._messageBookmarksSubject.next(this._messageBookmarks);      // Emit to observable
    }

    public get HasMessageBookmarks(): Promise<boolean> {
        return this.MessageBookmarks.then(messageBookmarks => messageBookmarks.length > 0);
    }


    /**
     *
     * Gets the MessageFlags for this ConversationMessage.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.conversationMessage.MessageFlags.then(conversationMessages => { ... })
     *   or
     *   await this.conversationMessage.conversationMessages
     *
    */
    public get MessageFlags(): Promise<MessageFlagData[]> {
        if (this._messageFlags !== null) {
            return Promise.resolve(this._messageFlags);
        }

        if (this._messageFlagsPromise !== null) {
            return this._messageFlagsPromise;
        }

        // Start the load
        this.loadMessageFlags();

        return this._messageFlagsPromise!;
    }



    private loadMessageFlags(): void {

        this._messageFlagsPromise = lastValueFrom(
            ConversationMessageService.Instance.GetMessageFlagsForConversationMessage(this.id)
        )
        .then(MessageFlags => {
            this._messageFlags = MessageFlags ?? [];
            this._messageFlagsSubject.next(this._messageFlags);
            return this._messageFlags;
         })
        .catch(err => {
            this._messageFlags = [];
            this._messageFlagsSubject.next(this._messageFlags);
            throw err;
        })
        .finally(() => {
            this._messageFlagsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MessageFlag. Call after mutations to force refresh.
     */
    public ClearMessageFlagsCache(): void {
        this._messageFlags = null;
        this._messageFlagsPromise = null;
        this._messageFlagsSubject.next(this._messageFlags);      // Emit to observable
    }

    public get HasMessageFlags(): Promise<boolean> {
        return this.MessageFlags.then(messageFlags => messageFlags.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (conversationMessage.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await conversationMessage.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ConversationMessageData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ConversationMessageData>> {
        const info = await lastValueFrom(
            ConversationMessageService.Instance.GetConversationMessageChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ConversationMessageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ConversationMessageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ConversationMessageSubmitData {
        return ConversationMessageService.Instance.ConvertToConversationMessageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ConversationMessageService extends SecureEndpointBase {

    private static _instance: ConversationMessageService;
    private listCache: Map<string, Observable<Array<ConversationMessageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ConversationMessageBasicListData>>>;
    private recordCache: Map<string, Observable<ConversationMessageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private conversationMessageChangeHistoryService: ConversationMessageChangeHistoryService,
        private conversationMessageAttachmentService: ConversationMessageAttachmentService,
        private conversationMessageUserService: ConversationMessageUserService,
        private conversationMessageReactionService: ConversationMessageReactionService,
        private conversationPinService: ConversationPinService,
        private conversationMessageLinkPreviewService: ConversationMessageLinkPreviewService,
        private conversationThreadUserService: ConversationThreadUserService,
        private messageBookmarkService: MessageBookmarkService,
        private messageFlagService: MessageFlagService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ConversationMessageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ConversationMessageBasicListData>>>();
        this.recordCache = new Map<string, Observable<ConversationMessageData>>();

        ConversationMessageService._instance = this;
    }

    public static get Instance(): ConversationMessageService {
      return ConversationMessageService._instance;
    }


    public ClearListCaches(config: ConversationMessageQueryParameters | null = null) {

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


    public ConvertToConversationMessageSubmitData(data: ConversationMessageData): ConversationMessageSubmitData {

        let output = new ConversationMessageSubmitData();

        output.id = data.id;
        output.conversationId = data.conversationId;
        output.userId = data.userId;
        output.parentConversationMessageId = data.parentConversationMessageId;
        output.conversationChannelId = data.conversationChannelId;
        output.dateTimeCreated = data.dateTimeCreated;
        output.message = data.message;
        output.messageType = data.messageType;
        output.entity = data.entity;
        output.entityId = data.entityId;
        output.externalURL = data.externalURL;
        output.forwardedFromMessageId = data.forwardedFromMessageId;
        output.forwardedFromUserId = data.forwardedFromUserId;
        output.isScheduled = data.isScheduled;
        output.scheduledDateTime = data.scheduledDateTime;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetConversationMessage(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const conversationMessage$ = this.requestConversationMessage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessage", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, conversationMessage$);

            return conversationMessage$;
        }

        return this.recordCache.get(configHash) as Observable<ConversationMessageData>;
    }

    private requestConversationMessage(id: bigint | number, includeRelations: boolean = true) : Observable<ConversationMessageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageData>(this.baseUrl + 'api/ConversationMessage/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveConversationMessage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessage(id, includeRelations));
            }));
    }

    public GetConversationMessageList(config: ConversationMessageQueryParameters | any = null) : Observable<Array<ConversationMessageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const conversationMessageList$ = this.requestConversationMessageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessage list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, conversationMessageList$);

            return conversationMessageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ConversationMessageData>>;
    }


    private requestConversationMessageList(config: ConversationMessageQueryParameters | any) : Observable <Array<ConversationMessageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageData>>(this.baseUrl + 'api/ConversationMessages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveConversationMessageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessageList(config));
            }));
    }

    public GetConversationMessagesRowCount(config: ConversationMessageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const conversationMessagesRowCount$ = this.requestConversationMessagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, conversationMessagesRowCount$);

            return conversationMessagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestConversationMessagesRowCount(config: ConversationMessageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ConversationMessages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessagesRowCount(config));
            }));
    }

    public GetConversationMessagesBasicListData(config: ConversationMessageQueryParameters | any = null) : Observable<Array<ConversationMessageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const conversationMessagesBasicListData$ = this.requestConversationMessagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ConversationMessages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, conversationMessagesBasicListData$);

            return conversationMessagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ConversationMessageBasicListData>>;
    }


    private requestConversationMessagesBasicListData(config: ConversationMessageQueryParameters | any) : Observable<Array<ConversationMessageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ConversationMessageBasicListData>>(this.baseUrl + 'api/ConversationMessages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestConversationMessagesBasicListData(config));
            }));

    }


    public PutConversationMessage(id: bigint | number, conversationMessage: ConversationMessageSubmitData) : Observable<ConversationMessageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationMessageData>(this.baseUrl + 'api/ConversationMessage/' + id.toString(), conversationMessage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutConversationMessage(id, conversationMessage));
            }));
    }


    public PostConversationMessage(conversationMessage: ConversationMessageSubmitData) : Observable<ConversationMessageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ConversationMessageData>(this.baseUrl + 'api/ConversationMessage', conversationMessage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostConversationMessage(conversationMessage));
            }));
    }

  
    public DeleteConversationMessage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ConversationMessage/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteConversationMessage(id));
            }));
    }

    public RollbackConversationMessage(id: bigint | number, versionNumber: bigint | number) : Observable<ConversationMessageData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ConversationMessageData>(this.baseUrl + 'api/ConversationMessage/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveConversationMessage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackConversationMessage(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ConversationMessage.
     */
    public GetConversationMessageChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ConversationMessageData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationMessageData>>(this.baseUrl + 'api/ConversationMessage/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ConversationMessage.
     */
    public GetConversationMessageAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ConversationMessageData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ConversationMessageData>[]>(this.baseUrl + 'api/ConversationMessage/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ConversationMessage.
     */
    public GetConversationMessageVersion(id: bigint | number, version: number): Observable<ConversationMessageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageData>(this.baseUrl + 'api/ConversationMessage/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationMessage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ConversationMessage at a specific point in time.
     */
    public GetConversationMessageStateAtTime(id: bigint | number, time: string): Observable<ConversationMessageData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ConversationMessageData>(this.baseUrl + 'api/ConversationMessage/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveConversationMessage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetConversationMessageStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ConversationMessageQueryParameters | any): string {

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

    public userIsSchedulerConversationMessageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerConversationMessageReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ConversationMessages
        //
        if (userIsSchedulerConversationMessageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerConversationMessageReader = user.readPermission >= 50;
            } else {
                userIsSchedulerConversationMessageReader = false;
            }
        }

        return userIsSchedulerConversationMessageReader;
    }


    public userIsSchedulerConversationMessageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerConversationMessageWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ConversationMessages
        //
        if (userIsSchedulerConversationMessageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerConversationMessageWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerConversationMessageWriter = false;
          }      
        }

        return userIsSchedulerConversationMessageWriter;
    }

    public GetConversationMessageChangeHistoriesForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageChangeHistoryData[]> {
        return this.conversationMessageChangeHistoryService.GetConversationMessageChangeHistoryList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationMessageAttachmentsForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageAttachmentData[]> {
        return this.conversationMessageAttachmentService.GetConversationMessageAttachmentList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationMessageUsersForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageUserData[]> {
        return this.conversationMessageUserService.GetConversationMessageUserList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationMessageReactionsForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageReactionData[]> {
        return this.conversationMessageReactionService.GetConversationMessageReactionList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationPinsForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationPinData[]> {
        return this.conversationPinService.GetConversationPinList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationMessageLinkPreviewsForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationMessageLinkPreviewData[]> {
        return this.conversationMessageLinkPreviewService.GetConversationMessageLinkPreviewList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConversationThreadUserParentConversationMessagesForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConversationThreadUserData[]> {
        return this.conversationThreadUserService.GetConversationThreadUserList({
            parentConversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMessageBookmarksForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MessageBookmarkData[]> {
        return this.messageBookmarkService.GetMessageBookmarkList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMessageFlagsForConversationMessage(conversationMessageId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MessageFlagData[]> {
        return this.messageFlagService.GetMessageFlagList({
            conversationMessageId: conversationMessageId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ConversationMessageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ConversationMessageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ConversationMessageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveConversationMessage(raw: any): ConversationMessageData {
    if (!raw) return raw;

    //
    // Create a ConversationMessageData object instance with correct prototype
    //
    const revived = Object.create(ConversationMessageData.prototype) as ConversationMessageData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._conversationMessageChangeHistories = null;
    (revived as any)._conversationMessageChangeHistoriesPromise = null;
    (revived as any)._conversationMessageChangeHistoriesSubject = new BehaviorSubject<ConversationMessageChangeHistoryData[] | null>(null);

    (revived as any)._conversationMessageAttachments = null;
    (revived as any)._conversationMessageAttachmentsPromise = null;
    (revived as any)._conversationMessageAttachmentsSubject = new BehaviorSubject<ConversationMessageAttachmentData[] | null>(null);

    (revived as any)._conversationMessageUsers = null;
    (revived as any)._conversationMessageUsersPromise = null;
    (revived as any)._conversationMessageUsersSubject = new BehaviorSubject<ConversationMessageUserData[] | null>(null);

    (revived as any)._conversationMessageReactions = null;
    (revived as any)._conversationMessageReactionsPromise = null;
    (revived as any)._conversationMessageReactionsSubject = new BehaviorSubject<ConversationMessageReactionData[] | null>(null);

    (revived as any)._conversationPins = null;
    (revived as any)._conversationPinsPromise = null;
    (revived as any)._conversationPinsSubject = new BehaviorSubject<ConversationPinData[] | null>(null);

    (revived as any)._conversationMessageLinkPreviews = null;
    (revived as any)._conversationMessageLinkPreviewsPromise = null;
    (revived as any)._conversationMessageLinkPreviewsSubject = new BehaviorSubject<ConversationMessageLinkPreviewData[] | null>(null);

    (revived as any)._conversationThreadUserParentConversationMessages = null;
    (revived as any)._conversationThreadUserParentConversationMessagesPromise = null;
    (revived as any)._conversationThreadUserParentConversationMessagesSubject = new BehaviorSubject<ConversationThreadUserData[] | null>(null);

    (revived as any)._messageBookmarks = null;
    (revived as any)._messageBookmarksPromise = null;
    (revived as any)._messageBookmarksSubject = new BehaviorSubject<MessageBookmarkData[] | null>(null);

    (revived as any)._messageFlags = null;
    (revived as any)._messageFlagsPromise = null;
    (revived as any)._messageFlagsSubject = new BehaviorSubject<MessageFlagData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadConversationMessageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ConversationMessageChangeHistories$ = (revived as any)._conversationMessageChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessageChangeHistories === null && (revived as any)._conversationMessageChangeHistoriesPromise === null) {
                (revived as any).loadConversationMessageChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessageChangeHistoriesCount$ = null;


    (revived as any).ConversationMessageAttachments$ = (revived as any)._conversationMessageAttachmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessageAttachments === null && (revived as any)._conversationMessageAttachmentsPromise === null) {
                (revived as any).loadConversationMessageAttachments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessageAttachmentsCount$ = null;


    (revived as any).ConversationMessageUsers$ = (revived as any)._conversationMessageUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessageUsers === null && (revived as any)._conversationMessageUsersPromise === null) {
                (revived as any).loadConversationMessageUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessageUsersCount$ = null;


    (revived as any).ConversationMessageReactions$ = (revived as any)._conversationMessageReactionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessageReactions === null && (revived as any)._conversationMessageReactionsPromise === null) {
                (revived as any).loadConversationMessageReactions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessageReactionsCount$ = null;


    (revived as any).ConversationPins$ = (revived as any)._conversationPinsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationPins === null && (revived as any)._conversationPinsPromise === null) {
                (revived as any).loadConversationPins();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationPinsCount$ = null;


    (revived as any).ConversationMessageLinkPreviews$ = (revived as any)._conversationMessageLinkPreviewsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationMessageLinkPreviews === null && (revived as any)._conversationMessageLinkPreviewsPromise === null) {
                (revived as any).loadConversationMessageLinkPreviews();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationMessageLinkPreviewsCount$ = null;


    (revived as any).ConversationThreadUserParentConversationMessages$ = (revived as any)._conversationThreadUserParentConversationMessagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._conversationThreadUserParentConversationMessages === null && (revived as any)._conversationThreadUserParentConversationMessagesPromise === null) {
                (revived as any).loadConversationThreadUserParentConversationMessages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._conversationThreadUserParentConversationMessagesCount$ = null;


    (revived as any).MessageBookmarks$ = (revived as any)._messageBookmarksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._messageBookmarks === null && (revived as any)._messageBookmarksPromise === null) {
                (revived as any).loadMessageBookmarks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._messageBookmarksCount$ = null;


    (revived as any).MessageFlags$ = (revived as any)._messageFlagsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._messageFlags === null && (revived as any)._messageFlagsPromise === null) {
                (revived as any).loadMessageFlags();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._messageFlagsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ConversationMessageData> | null>(null);

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

  private ReviveConversationMessageList(rawList: any[]): ConversationMessageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveConversationMessage(raw));
  }

}
