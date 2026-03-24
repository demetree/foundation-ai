import { Component, OnInit, OnDestroy, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ElementRef, HostListener, ChangeDetectorRef } from '@angular/core';
import { TiptapEditorComponent } from '../tiptap-editor/tiptap-editor.component';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { Subscription, Subject, forkJoin, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, catchError, map } from 'rxjs/operators';

import { AuthService } from '../../services/auth.service';
import {
  MessagingApiService, ConversationSummary, MessageSummary,
  ReactionSummary, PresenceSummary, ChannelSummary, BookmarkSummary
} from '../../services/messaging-api.service';
import {
  MessagingSignalRService, MessagePayload, MessageEditPayload,
  MessageDeletePayload, TypingPayload, ReactionPayload, PresencePayload,
  ReadReceiptPayload, ChannelPayload
} from '../../services/messaging-signalr.service';
import { UserData } from '../messaging/messaging.component';
import { ToastNotificationService } from '../../services/toast-notification.service';
import { CallService } from '../../services/call.service';


//
// Curated emoji set for reactions (no external picker dependency)
//
const REACTION_EMOJIS = ['👍', '❤️', '😂', '😮', '😢', '🎉'];


@Component({
  selector: 'app-chat-view',
  templateUrl: './chat-view.component.html',
  styleUrls: ['./chat-view.component.scss']
})
export class ChatViewComponent implements OnInit, OnDestroy, OnChanges {
  @ViewChild(CdkVirtualScrollViewport) virtualScroll!: CdkVirtualScrollViewport;
  @ViewChild('messageContainer') messageContainer!: ElementRef;
  @ViewChild('tiptapEditor') tiptapEditor!: TiptapEditorComponent;
  @ViewChild('fileInput') fileInput!: ElementRef;

  //
  // Inputs
  //
  @Input() conversation: ConversationSummary | null = null;
  @Input() displayMode: 'panel' | 'fullscreen' | 'popout' = 'panel';
  @Input() allConversations: ConversationSummary[] = [];

  //
  // Outputs
  //
  @Output() conversationUpdated = new EventEmitter<ConversationSummary>();
  @Output() backRequested = new EventEmitter<void>();

  //
  // Messages
  //
  messages: MessageSummary[] = [];
  messageText = '';
  editorHTML = '';
  editorIsEmpty = true;
  isLoadingMessages = false;
  typingUsers: Map<number, { displayName: string; timeout: any }> = new Map();

  //
  // @Mentions
  //
  showMentionPicker = false;
  mentionFilteredUsers: UserData[] = [];
  mentionStartIndex = -1;
  mentionActiveIndex = 0;
  allUsers: UserData[] = [];
  currentUserDisplayName: string = '';
  isLoadingUsers = false;

  //
  // Edit & Delete
  //
  editingMessageId: number | null = null;
  editingText = '';
  hoveredMessageId: number | null = null;

  //
  // Reactions
  //
  reactionPickerMessageId: number | null = null;
  availableEmojis = REACTION_EMOJIS;

  //
  // Search
  //
  isSearchMode = false;
  searchQuery = '';
  searchResults: MessageSummary[] = [];
  isSearching = false;
  private searchSubject = new Subject<string>();

  //
  // Infinite Scroll
  //
  hasMoreMessages = true;
  isLoadingMore = false;
  private oldestMessageDate: string | null = null;

  //
  // Presence
  //
  presenceMap: Map<number, string> = new Map();
  memberPresenceDetails: Map<number, PresenceSummary> = new Map();

  //
  // Read Receipts
  //
  readReceipts: Map<number, string[]> = new Map();

  //
  // Forward
  //
  showForwardModal = false;
  forwardingMessage: MessageSummary | null = null;

  //
  // Conversation Info
  //
  showConversationInfo = false;
  currentUserMessagingId: number | null = null;
  newMemberAccountName = '';
  memberSearchQuery = '';
  filteredMemberCandidates: UserData[] = [];
  showMemberDropdown = false;

  //
  // Attachments
  //
  pendingAttachments: File[] = [];
  isDragOver = false;
  thumbnailCache: Map<string, string> = new Map();
  lightboxImageUrl: string | null = null;

  //
  // Threads
  //
  threadParentMessage: MessageSummary | null = null;

  //
  // Sub-Channels (within a conversation)
  //
  channels: ChannelSummary[] = [];
  selectedChannel: ChannelSummary | null = null;
  showCreateChannel = false;
  newChannelName = '';
  newChannelTopic = '';
  newChannelIsPrivate = false;

  //
  // Pinned Messages
  //
  pinnedMessages: MessageSummary[] = [];
  pinnedMessageIds: Map<number, number> = new Map();  // messageId → pinId
  showPinnedMessages = false;
  isLoadingPinnedMessages = false;

  //
  // Bookmarks
  //
  bookmarkedMessageIds: Set<number> = new Set();
  bookmarks: BookmarkSummary[] = [];
  showBookmarks = false;
  isLoadingBookmarks = false;

  //
  // Schedule Send
  //
  showScheduleModal = false;
  scheduleDateTime = '';

  private subscriptions: Subscription[] = [];
  private typingTimeout: any = null;
  private isCurrentlyTyping = false;
  private draftSaveTimeout: any = null;


  constructor(
    private messagingApi: MessagingApiService,
    private signalR: MessagingSignalRService,
    private authService: AuthService,
    private toastService: ToastNotificationService,
    private cdr: ChangeDetectorRef,
    public callService: CallService
  ) {
  }


  ngOnInit(): void {
    this.subscribeToEvents();

    //
    // Debounced search
    //
    this.subscriptions.push(
      this.searchSubject.pipe(
        debounceTime(300),
        distinctUntilChanged()
      ).subscribe(query => {
        this.executeSearch(query);
      })
    );

    //
    // Load user's bookmarks at startup
    //
    this.loadBookmarks();
  }


  ngOnChanges(changes: SimpleChanges): void {
    if (changes['conversation'] && this.conversation) {
      this.onConversationChanged();
    }
  }


  private previousConversationId: number | null = null;

  private onConversationChanged(): void {
    if (!this.conversation) return;

    //
    // NOTE: Do NOT call leaveConversation() here — see ngOnDestroy comment.
    // All conversation groups remain joined on the shared singleton connection.
    //

    //
    // Reset chat state for the new conversation
    //
    this.messages = [];
    this.typingUsers.clear();
    this.hasMoreMessages = true;
    this.oldestMessageDate = null;
    this.showConversationInfo = false;
    this.editingMessageId = null;
    this.readReceipts.clear();
    this.channels = [];
    this.selectedChannel = null;
    this.showCreateChannel = false;
    this.threadParentMessage = null;
    this.showPinnedMessages = false;
    this.pinnedMessages = [];          // fix #11 — clear stale pins
    this.pinnedMessageIds.clear();     // fix #11

    //
    // Save draft from previous conversation before switching
    //
    this.saveDraftNow();

    //
    // Load draft for the new conversation
    //
    this.messageText = this.loadDraft(this.conversation.id);

    //
    // Resolve the current user's messaging DB userId from the conversation members.
    //
    this.currentUserMessagingId = null;
    this.currentUserDisplayName = '';
    const currentUser = this.authService.currentUser;
    if (currentUser && this.conversation.members) {
      this.currentUserDisplayName = currentUser.fullName || '';
      const me = this.conversation.members.find(m =>
        m.accountName?.toLowerCase() === currentUser.userName?.toLowerCase()
      );
      if (me) {
        this.currentUserMessagingId = me.userId;
      }
    }

    //
    // Join the conversation SignalR group and load data
    //
    this.signalR.joinConversation(this.conversation.id);
    this.previousConversationId = this.conversation.id;
    this.loadMessages(this.conversation.id);
    this.loadPresence(this.conversation.id);
    this.loadChannels(this.conversation.id);
    this.loadPinnedMessages();
  }


  // ─── SignalR Event Subscriptions ───────────────────────────────────────────

  private subscribeToEvents(): void {
    this.subscriptions.push(
      this.signalR.messageReceived$.subscribe(payload => this.handleNewMessage(payload))
    );
    this.subscriptions.push(
      this.signalR.messageEdited$.subscribe(payload => this.handleMessageEdited(payload))
    );
    this.subscriptions.push(
      this.signalR.messageDeleted$.subscribe(payload => this.handleMessageDeleted(payload))
    );
    this.subscriptions.push(
      this.signalR.typingStarted$.subscribe(payload => this.handleTypingStarted(payload))
    );
    this.subscriptions.push(
      this.signalR.typingStopped$.subscribe(payload => this.handleTypingStopped(payload))
    );
    this.subscriptions.push(
      this.signalR.reactionAdded$.subscribe(payload => this.handleReactionAdded(payload))
    );
    this.subscriptions.push(
      this.signalR.reactionRemoved$.subscribe(payload => this.handleReactionRemoved(payload))
    );
    this.subscriptions.push(
      this.signalR.presenceChanged$.subscribe(payload => this.handlePresenceChanged(payload))
    );
    this.subscriptions.push(
      this.signalR.messageRead$.subscribe(payload => this.handleReadReceipt(payload))
    );
    this.subscriptions.push(
      this.signalR.channelCreated$.subscribe(payload => this.handleChannelCreated(payload))
    );
    this.subscriptions.push(
      this.signalR.channelUpdated$.subscribe(payload => this.handleChannelUpdated(payload))
    );
    this.subscriptions.push(
      this.signalR.channelDeleted$.subscribe(payload => this.handleChannelDeleted(payload))
    );
    this.subscriptions.push(
      this.signalR.linkPreviewReady$.subscribe(payload => this.handleLinkPreviewReady(payload))
    );
  }


  // ─── Messages ───────────────────────────────────────────────────────────────

  private loadMessages(conversationId: number): void {
    this.isLoadingMessages = true;
    const channelId = this.selectedChannel?.id;

    this.messagingApi.getMessages(conversationId, undefined, 50, channelId).subscribe({
      next: (messages) => {
        this.messages = messages;
        this.isLoadingMessages = false;
        this.hasMoreMessages = messages.length >= 50;

        if (this.messages.length > 0) {
          this.oldestMessageDate = this.messages[0].dateTimeCreated;
        }

        //
        // Instant scroll-to-bottom:
        //   Hide the viewport, position to the bottom while invisible,
        //   then reveal — so the conversation appears to open at the
        //   bottom like Teams / iMessage (no visible scroll animation).
        //
        const viewportEl = this.virtualScroll?.elementRef?.nativeElement;
        if (viewportEl) {
          viewportEl.style.opacity = '0';
        }
        setTimeout(() => {
          this.scrollToBottom();
          setTimeout(() => {
            if (viewportEl) { viewportEl.style.opacity = '1'; }
          }, 50);
        }, 150);

        //
        // Load image thumbnails
        //
        this.loadThumbnails(messages);

        //
        // Mark ALL messages in the conversation as read
        //
        if (this.messages.length > 0) {
          this.messagingApi.markConversationRead(conversationId).subscribe();

          if (this.conversation != null) {
            this.conversation.unreadCount = 0;
            this.conversationUpdated.emit(this.conversation);
          }
        }
      },
      error: (error) => {
        console.error('Failed to load messages', error);
        this.isLoadingMessages = false;
      }
    });
  }

  private pendingMessageCounter = -1;

  sendMessage(): void {
    const html = this.tiptapEditor ? this.tiptapEditor.getHTML() : '';
    const plainText = this.tiptapEditor ? this.tiptapEditor.getText().trim() : this.messageText.trim();
    const hasText = plainText.length > 0;
    const hasAttachments = this.pendingAttachments.length > 0;

    if ((!hasText && !hasAttachments) || this.conversation == null) {
      return;
    }

    // Use the rich HTML content as the message
    const text = html;
    this.messageText = '';
    this.editorHTML = '';
    this.editorIsEmpty = true;
    if (this.tiptapEditor) {
      this.tiptapEditor.clearContent();
    }

    this.stopTyping();

    //
    // Clear draft on send
    //
    this.clearDraft(this.conversation.id);

    if (hasAttachments) {
      this.uploadAndSendMessage(text);
      return;
    }

    //
    // Optimistic insert: show message immediately with a pending state
    //
    const tempId = this.pendingMessageCounter--;
    const pendingMessage: MessageSummary = {
      id: tempId,
      conversationId: this.conversation.id,
      conversationChannelId: this.selectedChannel?.id ?? null,
      userId: this.currentUserMessagingId ?? 0,
      userDisplayName: this.authService.currentUser?.fullName || 'You',
      message: text,
      dateTimeCreated: new Date().toISOString(),
      parentConversationMessageId: null,
      entity: null,
      entityId: null,
      externalURL: null,
      versionNumber: 1,
      acknowledged: true,
      replyCount: 0,
      attachments: [],
      reactions: [],
      isPending: true
    };

    this.messages.push(pendingMessage);
    this.messages = [...this.messages];
    this.scrollToBottom();

    this.messagingApi.sendMessage(this.conversation.id, text, undefined, undefined, undefined, undefined, this.selectedChannel?.id).subscribe({
      next: (message) => {
        // Replace the pending message with the real one from the server
        const pendingIndex = this.messages.findIndex(m => m.id === tempId);
        if (pendingIndex >= 0) {
          this.messages[pendingIndex] = message;
          this.messages = [...this.messages];
        } else {
          this.mergeOrAddMessage(message);
        }
      },
      error: (error) => {
        console.error('Failed to send message', error);
        // Mark the pending message as failed instead of removing it
        const pendingIndex = this.messages.findIndex(m => m.id === tempId);
        if (pendingIndex >= 0) {
          this.messages[pendingIndex].isPending = false;
          this.messages[pendingIndex].isFailed = true;
          this.messages = [...this.messages];
        }
      }
    });
  }

  retryMessage(msg: MessageSummary): void {
    if (!msg.isFailed || this.conversation == null) return;

    const text = msg.message;
    const failedIndex = this.messages.findIndex(m => m.id === msg.id);

    // Reset to pending state
    if (failedIndex >= 0) {
      this.messages[failedIndex].isFailed = false;
      this.messages[failedIndex].isPending = true;
      this.messages = [...this.messages];
    }

    this.messagingApi.sendMessage(this.conversation.id, text, undefined, undefined, undefined, undefined, this.selectedChannel?.id).subscribe({
      next: (message) => {
        const idx = this.messages.findIndex(m => m.id === msg.id);
        if (idx >= 0) {
          this.messages[idx] = message;
          this.messages = [...this.messages];
        }
      },
      error: () => {
        const idx = this.messages.findIndex(m => m.id === msg.id);
        if (idx >= 0) {
          this.messages[idx].isPending = false;
          this.messages[idx].isFailed = true;
          this.messages = [...this.messages];
        }
      }
    });
  }

  discardFailedMessage(msg: MessageSummary): void {
    this.messages = this.messages.filter(m => m.id !== msg.id);
  }

  private uploadAndSendMessage(text: string): void {
    const uploads = [...this.pendingAttachments];
    this.pendingAttachments = [];

    const uploadObservables = uploads.map(file =>
      this.messagingApi.uploadAttachment(file).pipe(
        map(result => {
          if (result && result.attachmentGuid) {
            return {
              attachmentGuid: result.attachmentGuid,
              fileName: result.fileName,
              mimeType: result.mimeType,
              contentSize: result.contentSize
            };
          }
          return null;
        }),
        catchError(error => {
          console.error('Failed to upload attachment', error);
          return of(null);
        })
      )
    );

    forkJoin(uploadObservables).subscribe({
      next: (results) => {
        const collectedAttachments = results.filter(r => r != null) as { attachmentGuid: string; fileName: string; mimeType: string; contentSize: number }[];

        if (this.conversation != null) {
          this.messagingApi.sendMessage(this.conversation.id, text, undefined, undefined, undefined, collectedAttachments.length > 0 ? collectedAttachments : undefined, this.selectedChannel?.id).subscribe({
            next: (message) => {
              this.mergeOrAddMessage(message);
            },
            error: (error) => {
              console.error('Failed to send message', error);
              this.messageText = text;
            }
          });
        }
      },
      error: (error) => {
        console.error('Failed to upload attachments', error);
        this.messageText = text;
      }
    });
  }

  @HostListener('document:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    //
    // Mention picker keyboard navigation
    // This HostListener captures keydown events when the mention picker is visible
    //
    if (this.showMentionPicker && this.mentionFilteredUsers.length > 0) {
      if (event.key === 'ArrowDown') {
        event.preventDefault();
        this.mentionActiveIndex = Math.min(this.mentionActiveIndex + 1, this.mentionFilteredUsers.length - 1);
        return;
      }
      if (event.key === 'ArrowUp') {
        event.preventDefault();
        this.mentionActiveIndex = Math.max(this.mentionActiveIndex - 1, 0);
        return;
      }
      if (event.key === 'Enter' || event.key === 'Tab') {
        event.preventDefault();
        this.insertMention(this.mentionFilteredUsers[this.mentionActiveIndex]);
        return;
      }
      if (event.key === 'Escape') {
        event.preventDefault();
        this.showMentionPicker = false;
        this.mentionFilteredUsers = [];
        return;
      }
    }
  }

  private stopTyping(): void {
    if (this.isCurrentlyTyping && this.conversation != null) {
      this.isCurrentlyTyping = false;
      const user = this.authService.currentUser;

      if (user != null && this.currentUserMessagingId != null) {
        this.signalR.sendStoppedTyping(this.conversation.id, this.currentUserMessagingId, user.fullName);
      }
    }

    if (this.typingTimeout != null) {
      clearTimeout(this.typingTimeout);
      this.typingTimeout = null;
    }
  }


  // ─── @Mention Picker ───────────────────────────────────────────────────────

  // ─── Tiptap Editor Bridge ──────────────────────────────────────────────────

  onEditorContentChanged(html: string): void {
    this.editorHTML = html;
    this.editorIsEmpty = this.tiptapEditor ? this.tiptapEditor.isEmpty() : true;
    this.scheduleDraftSave();
  }

  onEditorMentionTriggered(event: { query: string; from: number }): void {
    const query = event.query;

    if (this.allUsers.length === 0) {
      this.messagingApi.getAllUserPresences().subscribe({
        next: (presences: PresenceSummary[]) => {
          const currentUser = this.authService.currentUser;
          this.allUsers = presences
            .filter((p: PresenceSummary) => currentUser == null || p.accountName !== currentUser.userName)
            .map((p: PresenceSummary) => ({ id: p.userId, accountName: p.accountName, displayName: p.displayName }));
          this.filterMentionUsers(query);
        }
      });
    } else {
      this.filterMentionUsers(query);
    }
  }

  onEditorMentionDismissed(): void {
    this.showMentionPicker = false;
    this.mentionFilteredUsers = [];
  }

  onEditorKeyPressed(): void {
    if (this.isCurrentlyTyping === false && this.conversation != null) {
      this.isCurrentlyTyping = true;
      const user = this.authService.currentUser;
      if (user != null && this.currentUserMessagingId != null) {
        this.signalR.sendTypingIndicator(this.conversation.id, this.currentUserMessagingId, user.fullName);
      }
    }
    if (this.typingTimeout != null) {
      clearTimeout(this.typingTimeout);
    }
    this.typingTimeout = setTimeout(() => this.stopTyping(), 3000);
  }

  // Legacy textarea handler — kept for backward compat but now unused by primary editor
  onComposerInput(event: Event): void { }

  private filterMentionUsers(query: string): void {
    if (query.length === 0) {
      this.mentionFilteredUsers = this.allUsers.slice(0, 8);
    } else {
      this.mentionFilteredUsers = this.allUsers.filter(u => {
        const name = (u.displayName || '').toLowerCase();
        const account = (u.accountName || '').toLowerCase();
        return name.includes(query) || account.includes(query);
      }).slice(0, 8);
    }
    this.mentionActiveIndex = 0;
    this.showMentionPicker = this.mentionFilteredUsers.length > 0;
  }

  insertMention(user: UserData): void {
    const displayName = user.displayName || user.accountName;

    // Use Tiptap editor's insertMention if available
    if (this.tiptapEditor) {
      this.tiptapEditor.insertMentionAtCursor(displayName);
    }

    this.showMentionPicker = false;
    this.mentionFilteredUsers = [];
  }


  // ─── Edit & Delete ─────────────────────────────────────────────────────────

  startEdit(msg: MessageSummary): void {
    this.editingMessageId = msg.id;
    this.editingText = msg.message;
    this.reactionPickerMessageId = null;
  }

  saveEdit(): void {
    if (this.editingMessageId == null || this.editingText.trim().length === 0) {
      return;
    }

    const messageId = this.editingMessageId;
    const newContent = this.editingText.trim();

    this.messagingApi.editMessage(messageId, newContent).subscribe({
      next: () => {
        const msg = this.messages.find(m => m.id === messageId);
        if (msg != null) {
          msg.message = newContent;
        }
        this.cancelEdit();
      },
      error: (error) => {
        console.error('Failed to edit message', error);
      }
    });
  }

  cancelEdit(): void {
    this.editingMessageId = null;
    this.editingText = '';
  }

  deleteMessage(msg: MessageSummary): void {
    if (!confirm('Delete this message? This cannot be undone.')) return;

    this.messagingApi.deleteMessage(msg.id).subscribe({
      next: () => {
        this.markMessageAsDeleted(msg.id);
      },
      error: (error: any) => {
        console.error('Failed to delete message', error);
      }
    });
  }


  private markMessageAsDeleted(messageId: number): void {
    const msg = this.messages.find(m => m.id === messageId);
    if (msg != null) {
      msg.isDeleted = true;
      msg.message = '';
      msg.attachments = [];
    }
  }


  // ─── Forward ─────────────────────────────────────────────────────────────

  openForwardPicker(msg: MessageSummary): void {
    this.forwardingMessage = msg;
    this.showForwardModal = true;
  }

  onForwardComplete(): void {
    this.showForwardModal = false;
    this.forwardingMessage = null;
  }

  onEditKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.saveEdit();
    }
    else if (event.key === 'Escape') {
      this.cancelEdit();
    }
  }


  // ─── Reactions ─────────────────────────────────────────────────────────────

  toggleReactionPicker(messageId: number): void {
    this.reactionPickerMessageId = this.reactionPickerMessageId === messageId ? null : messageId;
  }

  addReaction(messageId: number, emoji: string): void {
    this.reactionPickerMessageId = null;

    this.messagingApi.addReaction(messageId, emoji).subscribe({
      next: () => {
        const msg = this.messages.find(m => m.id === messageId);
        if (msg != null) {
          this.updateLocalReaction(msg, emoji, true);
        }
      },
      error: (error) => {
        console.error('Failed to add reaction', error);
      }
    });
  }

  toggleReaction(messageId: number, emoji: string): void {
    const msg = this.messages.find(m => m.id === messageId);
    if (msg == null) return;

    const currentUser = this.authService.currentUser;
    if (currentUser == null) return;

    const existingReaction = msg.reactions.find(r => r.reaction === emoji);
    const hasMyReaction = existingReaction != null && existingReaction.currentUserReacted;

    this.messagingApi.toggleReaction(messageId, emoji).subscribe({
      next: (result) => {
        if (result?.action === 'removed') {
          this.updateLocalReaction(msg, emoji, false);
        } else {
          this.updateLocalReaction(msg, emoji, true);
        }
      },
      error: (error) => console.error('Failed to toggle reaction', error)
    });
  }

  private updateLocalReaction(msg: MessageSummary, emoji: string, add: boolean): void {
    const currentUser = this.authService.currentUser;
    if (currentUser == null) return;

    let existing = msg.reactions.find(r => r.reaction === emoji);

    if (add) {
      if (existing != null) {
        if (!existing.userNames.includes(currentUser.fullName)) {
          existing.count++;
          existing.userNames.push(currentUser.fullName);
          existing.currentUserReacted = true;
        }
      }
      else {
        msg.reactions.push({ reaction: emoji, count: 1, userNames: [currentUser.fullName], currentUserReacted: true });
      }
    }
    else {
      if (existing != null) {
        existing.count--;
        existing.userNames = existing.userNames.filter((n: string) => n !== currentUser.fullName);
        existing.currentUserReacted = false;
        if (existing.count <= 0) {
          msg.reactions = msg.reactions.filter(r => r.reaction !== emoji);
        }
      }
    }
  }


  // ─── Link Previews ─────────────────────────────────────────────────────────

  dismissLinkPreview(previewId: number, msg: any): void {
    // Optimistically remove from local UI
    if (msg.linkPreviews) {
      msg.linkPreviews = msg.linkPreviews.filter((p: any) => p.id !== previewId);
    }

    // Persist dismissal on backend
    this.messagingApi.dismissLinkPreview(previewId).subscribe({
      error: (err: any) => console.error('Failed to dismiss link preview', err)
    });
  }

  handleLinkPreviewReady(payload: any): void {
    if (!this.conversation || payload.conversationId !== this.conversation.id) {
      return;
    }

    const msg = this.messages.find(m => m.id === payload.messageId);
    if (msg != null && payload.previews) {
      if (!msg.linkPreviews) {
        msg.linkPreviews = [];
      }
      msg.linkPreviews.push(...payload.previews);
    }
  }


  // ─── Search ────────────────────────────────────────────────────────────────

  toggleSearch(): void {
    this.isSearchMode = !this.isSearchMode;
    if (!this.isSearchMode) {
      this.searchQuery = '';
      this.searchResults = [];
    }
  }

  onSearchInput(): void {
    this.searchSubject.next(this.searchQuery);
  }

  private executeSearch(query: string): void {
    if (query.trim().length < 2) {
      this.searchResults = [];
      return;
    }

    this.isSearching = true;

    const conversationId = this.conversation?.id;
    this.messagingApi.searchMessages(query, conversationId).subscribe({
      next: (results) => {
        this.searchResults = results;
        this.isSearching = false;
      },
      error: (error) => {
        console.error('Failed to search messages', error);
        this.isSearching = false;
      }
    });
  }

  jumpToSearchResult(result: MessageSummary): void {
    this.isSearchMode = false;
    this.searchQuery = '';
    this.searchResults = [];

    setTimeout(() => {
      const el = document.getElementById(`msg-${result.id}`);
      if (el != null) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        el.classList.add('highlight-flash');
        setTimeout(() => el.classList.remove('highlight-flash'), 2000);
      }
    }, 200);
  }


  // ─── Infinite Scroll ──────────────────────────────────────────────────────

  onVirtualScroll(): void {
    const el = this.virtualScroll?.elementRef?.nativeElement;
    if (!el) return;

    if (el.scrollTop < 50 && this.hasMoreMessages && !this.isLoadingMore && this.conversation != null) {
      this.loadOlderMessages();
    }
  }

  private loadOlderMessages(): void {
    if (this.conversation == null || this.oldestMessageDate == null) {
      return;
    }

    this.isLoadingMore = true;
    const container = this.virtualScroll?.elementRef?.nativeElement;
    const previousScrollHeight = container?.scrollHeight || 0;

    this.messagingApi.getMessages(this.conversation.id, this.oldestMessageDate, 50, this.selectedChannel?.id).subscribe({
      next: (olderMessages) => {
        if (olderMessages.length < 50) {
          this.hasMoreMessages = false;
        }

        if (olderMessages.length > 0) {
          const reversed = olderMessages.reverse();
          this.messages = [...reversed, ...this.messages];
          this.oldestMessageDate = this.messages[0].dateTimeCreated;
          this.loadThumbnails(reversed);

          setTimeout(() => {
            if (container != null) {
              const newScrollHeight = container.scrollHeight;
              container.scrollTop = newScrollHeight - previousScrollHeight;
            }
          }, 10);
        }

        this.isLoadingMore = false;
      },
      error: (error) => {
        console.error('Failed to load older messages', error);
        this.isLoadingMore = false;
      }
    });
  }


  // ─── Presence ──────────────────────────────────────────────────────────────

  private loadPresence(conversationId: number): void {
    this.messagingApi.getConversationPresences(conversationId).subscribe({
      next: (presenceList) => {
        this.presenceMap.clear();
        for (const p of presenceList) {
          this.presenceMap.set(p.userId, p.status);
        }
      },
      error: (error) => {
        console.error('Failed to load presence', error);
      }
    });
  }

  getPresenceClass(userId: number): string {
    if (this.currentUserMessagingId != null && userId === this.currentUserMessagingId) {
      return 'online';
    }

    const status = this.presenceMap.get(userId);
    if (status == null) return 'offline';
    return status.toLowerCase();
  }

  getPresenceClassForMember(member: any): string {
    return this.getPresenceClass(member.userId);
  }


  // ─── Sub-Channels ─────────────────────────────────────────────────────────

  private loadChannels(conversationId: number): void {
    this.messagingApi.getChannels(conversationId).subscribe({
      next: (channels) => {
        this.channels = channels;
      },
      error: (error) => {
        console.error('Failed to load channels', error);
      }
    });
  }

  selectChannel(channel: ChannelSummary | null): void {
    this.selectedChannel = channel;

    if (this.conversation) {
      this.messages = [];
      this.hasMoreMessages = true;
      this.oldestMessageDate = null;
      this.loadMessages(this.conversation.id);
    }
  }

  clearChannelFilter(): void {
    this.selectChannel(null);
  }

  toggleCreateChannel(): void {
    this.showCreateChannel = !this.showCreateChannel;
    if (!this.showCreateChannel) {
      this.newChannelName = '';
      this.newChannelTopic = '';
      this.newChannelIsPrivate = false;
    }
  }

  createNewChannel(): void {
    if (this.conversation == null || this.newChannelName.trim().length === 0) return;

    this.messagingApi.createChannel(
      this.conversation.id,
      this.newChannelName.trim(),
      this.newChannelTopic.trim() || undefined,
      this.newChannelIsPrivate
    ).subscribe({
      next: (channel) => {
        this.channels.push(channel);
        this.showCreateChannel = false;
        this.newChannelName = '';
        this.newChannelTopic = '';
        this.newChannelIsPrivate = false;
        this.selectedChannel = channel;
      },
      error: (error) => {
        console.error('Failed to create channel', error);
      }
    });
  }

  deleteChannel(channelId: number): void {
    this.messagingApi.deleteChannel(channelId).subscribe({
      next: () => {
        this.channels = this.channels.filter(c => c.id !== channelId);
        if (this.selectedChannel?.id === channelId) {
          this.selectChannel(null);
        }
      },
      error: (error) => {
        console.error('Failed to delete channel', error);
      }
    });
  }

  private handleChannelCreated(payload: ChannelPayload): void {
    if (this.conversation == null || payload.conversationId !== this.conversation.id) return;

    if (!this.channels.some(c => c.id === payload.channelId)) {
      this.channels.push({
        id: payload.channelId,
        conversationId: payload.conversationId,
        name: payload.name,
        topic: payload.topic,
        isPrivate: payload.isPrivate,
        isPinned: payload.isPinned,
        objectGuid: '',
        messageCount: 0,
        lastMessagePreview: null
      });
    }
  }

  private handleChannelUpdated(payload: ChannelPayload): void {
    const channel = this.channels.find(c => c.id === payload.channelId);
    if (channel) {
      channel.name = payload.name;
      channel.topic = payload.topic;
      channel.isPrivate = payload.isPrivate;
      channel.isPinned = payload.isPinned;
    }
  }

  private handleChannelDeleted(payload: ChannelPayload): void {
    this.channels = this.channels.filter(c => c.id !== payload.channelId);
    if (this.selectedChannel?.id === payload.channelId) {
      this.selectChannel(null);
    }
  }


  // ─── Pinned Messages ──────────────────────────────────────────────────────

  loadPinnedMessages(): void {
    if (this.conversation == null) return;

    this.isLoadingPinnedMessages = true;

    this.messagingApi.getPinnedMessages(this.conversation.id).subscribe({
      next: (pins) => {
        this.pinnedMessageIds.clear();
        this.pinnedMessages = [];

        for (const pin of pins) {
          const messageId = pin.messageId ?? pin.conversationMessageId;
          const pinId = pin.id ?? pin.pinId;
          this.pinnedMessageIds.set(messageId, pinId);

          const msg = this.messages.find(m => m.id === messageId);
          if (msg) {
            this.pinnedMessages.push(msg);
          } else {
            // fix #12 — create stub for pinned messages older than the loaded window
            this.pinnedMessages.push({
              id: messageId,
              conversationId: pin.conversationId ?? this.conversation!.id,
              conversationChannelId: null,
              userId: pin.pinnedByUserId ?? 0,
              userDisplayName: pin.pinnedByUserName ?? 'Unknown',
              message: pin.messagePreview ?? '(pinned message)',
              dateTimeCreated: pin.dateTimePinned,
              parentConversationMessageId: null,
              entity: null,
              entityId: null,
              externalURL: null,
              versionNumber: 1,
              acknowledged: true,
              replyCount: 0,
              attachments: [],
              reactions: []
            } as MessageSummary);
          }
        }
        this.isLoadingPinnedMessages = false;
      },
      error: (error) => {
        console.error('Failed to load pinned messages', error);
        this.isLoadingPinnedMessages = false;
      }
    });
  }

  togglePinMessage(msg: MessageSummary): void {
    if (this.conversation == null) return;

    if (this.isPinnedMessage(msg.id)) {
      const pinId = this.pinnedMessageIds.get(msg.id);
      if (pinId == null) return;

      this.messagingApi.unpinMessage(pinId).subscribe({
        next: () => {
          this.pinnedMessageIds.delete(msg.id);
          this.pinnedMessages = this.pinnedMessages.filter(m => m.id !== msg.id);
        },
        error: (error) => {
          console.error('Failed to unpin message', error);
        }
      });
    } else {
      this.messagingApi.pinMessage(msg.id).subscribe({
        next: (result) => {
          const pinId = result?.id ?? result?.pinId ?? 0;
          this.pinnedMessageIds.set(msg.id, pinId);
          this.pinnedMessages.push(msg);
        },
        error: (error) => {
          console.error('Failed to pin message', error);
        }
      });
    }
  }

  isPinnedMessage(messageId: number): boolean {
    return this.pinnedMessageIds.has(messageId);
  }

  toggleShowPinnedMessages(): void {
    this.showPinnedMessages = !this.showPinnedMessages;
    if (this.showPinnedMessages && this.pinnedMessages.length === 0) {
      this.loadPinnedMessages();
    }
  }


  // ─── Bookmarks ──────────────────────────────────────────────────────────────

  private loadBookmarks(): void {
    this.messagingApi.getBookmarks().subscribe({
      next: (bookmarks) => {
        this.bookmarks = bookmarks;
        this.bookmarkedMessageIds.clear();
        for (const b of bookmarks) {
          this.bookmarkedMessageIds.add(b.conversationMessageId);
        }
      },
      error: (error) => {
        console.error('Failed to load bookmarks', error);
      }
    });
  }

  isBookmarked(messageId: number): boolean {
    return this.bookmarkedMessageIds.has(messageId);
  }

  toggleBookmark(msg: MessageSummary): void {
    if (this.isBookmarked(msg.id)) {
      const bookmark = this.bookmarks.find(b => b.conversationMessageId === msg.id);
      if (!bookmark) return;

      this.bookmarkedMessageIds.delete(msg.id);
      this.bookmarks = this.bookmarks.filter(b => b.conversationMessageId !== msg.id);

      this.messagingApi.removeBookmark(bookmark.id).subscribe({
        error: (error) => {
          // Revert on failure
          this.bookmarkedMessageIds.add(msg.id);
          this.bookmarks.push(bookmark);
          console.error('Failed to remove bookmark', error);
        }
      });
    } else {
      this.bookmarkedMessageIds.add(msg.id);

      this.messagingApi.addBookmark(msg.id).subscribe({
        next: (result) => {
          this.bookmarks.push({
            id: result.bookmarkId,
            conversationMessageId: msg.id,
            messagePreview: (msg.message || '').substring(0, 80),
            conversationId: msg.conversationId,
            conversationName: this.conversation?.name || null,
            note: null,
            dateTimeCreated: new Date().toISOString()
          });
          this.toastService.show('Message bookmarked');
        },
        error: (error) => {
          this.bookmarkedMessageIds.delete(msg.id);
          console.error('Failed to add bookmark', error);
        }
      });
    }
  }

  toggleShowBookmarks(): void {
    this.showBookmarks = !this.showBookmarks;
    if (this.showBookmarks) {
      this.showPinnedMessages = false;
      this.showConversationInfo = false;
      this.isLoadingBookmarks = true;
      this.messagingApi.getBookmarks().subscribe({
        next: (bookmarks) => {
          this.bookmarks = bookmarks;
          this.isLoadingBookmarks = false;
        },
        error: () => {
          this.isLoadingBookmarks = false;
        }
      });
    }
  }

  removeBookmarkFromPanel(bookmark: BookmarkSummary): void {
    this.bookmarkedMessageIds.delete(bookmark.conversationMessageId);
    this.bookmarks = this.bookmarks.filter(b => b.id !== bookmark.id);

    this.messagingApi.removeBookmark(bookmark.id).subscribe({
      error: (error) => console.error('Failed to remove bookmark', error)
    });
  }

  jumpToBookmarkedMessage(bookmark: BookmarkSummary): void {
    this.showBookmarks = false;

    if (bookmark.conversationId !== this.conversation?.id) {
      // Different conversation — can't jump. Just close panel.
      return;
    }

    setTimeout(() => {
      const el = document.getElementById('msg-' + bookmark.conversationMessageId);
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        el.classList.add('highlight-flash');
        setTimeout(() => el.classList.remove('highlight-flash'), 2000);
      }
    }, 200);
  }


  // ─── Mark Unread ─────────────────────────────────────────────────────────────

  markUnread(msg: MessageSummary): void {
    this.messagingApi.markMessageUnread(msg.id).subscribe({
      next: () => {
        if (this.conversation) {
          this.conversation.unreadCount = Math.max(this.conversation.unreadCount, 1);
          this.conversationUpdated.emit(this.conversation);
        }
        this.toastService.show('Marked as unread');
      },
      error: (error) => {
        console.error('Failed to mark as unread', error);
      }
    });
  }


  // ─── Schedule Send ──────────────────────────────────────────────────────────

  openScheduleModal(): void {
    //
    // Default to 1 hour from now
    //
    const oneHourLater = new Date(Date.now() + 60 * 60 * 1000);
    this.scheduleDateTime = oneHourLater.toISOString().slice(0, 16);
    this.showScheduleModal = true;
  }

  cancelSchedule(): void {
    this.showScheduleModal = false;
    this.scheduleDateTime = '';
  }

  confirmSchedule(): void {
    if (!this.scheduleDateTime || !this.conversation) return;

    const html = this.tiptapEditor ? this.tiptapEditor.getHTML() : '';
    const plainText = this.tiptapEditor ? this.tiptapEditor.getText().trim() : '';
    if (plainText.length === 0) return;

    const scheduledIso = new Date(this.scheduleDateTime).toISOString();

    this.messagingApi.scheduleMessage(
      this.conversation.id,
      html,
      scheduledIso
    ).subscribe({
      next: () => {
        this.toastService.show('Message scheduled');
        this.showScheduleModal = false;
        this.scheduleDateTime = '';
        this.messageText = '';
        this.editorHTML = '';
        this.editorIsEmpty = true;
        if (this.tiptapEditor) {
          this.tiptapEditor.clearContent();
        }
        this.clearDraft(this.conversation!.id);
      },
      error: (error) => {
        console.error('Failed to schedule message', error);
      }
    });
  }


  // ─── Role Management ────────────────────────────────────────────────────────

  updateMemberRole(userId: number, newRole: string): void {
    if (!this.conversation) return;

    this.messagingApi.updateUserRole(this.conversation.id, userId, newRole).subscribe({
      next: () => {
        const member = this.conversation?.members?.find(m => m.userId === userId);
        if (member) {
          member.role = newRole;
        }
        this.toastService.show(`Role updated to ${newRole}`);
      },
      error: (error) => {
        console.error('Failed to update role', error);
      }
    });
  }

  isCurrentUserAdmin(): boolean {
    if (!this.conversation?.members || !this.currentUserMessagingId) return false;
    const me = this.conversation.members.find(m => m.userId === this.currentUserMessagingId);
    return me?.role === 'Admin' || me?.role === 'Owner';
  }

  scrollToPinnedMessage(msg: MessageSummary): void {
    this.showPinnedMessages = false;
    const el = document.getElementById('msg-' + msg.id);
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' });
      el.classList.add('highlight-flash');
      setTimeout(() => el.classList.remove('highlight-flash'), 2000);
    }
  }


  // ─── Conversation Info ─────────────────────────────────────────────────────

  toggleConversationInfo(): void {
    this.showConversationInfo = !this.showConversationInfo;
    this.newMemberAccountName = '';
    this.memberSearchQuery = '';
    this.filteredMemberCandidates = [];
    this.showMemberDropdown = false;

    if (this.showConversationInfo) {
      this.refreshMemberList();
      this.loadMemberPresence();
    }

    if (this.showConversationInfo && this.allUsers.length === 0) {
      this.isLoadingUsers = true;
      this.messagingApi.getAllUserPresences().subscribe({
        next: (presences: PresenceSummary[]) => {
          this.allUsers = presences
            .filter((p: PresenceSummary) => {
              const currentUser = this.authService.currentUser;
              return currentUser == null || p.accountName !== currentUser.userName;
            })
            .map((p: PresenceSummary) => ({ id: p.userId, accountName: p.accountName, displayName: p.displayName }));
          this.isLoadingUsers = false;
        },
        error: () => {
          this.isLoadingUsers = false;
        }
      });
    }
  }

  onMemberSearchInput(): void {
    const query = this.memberSearchQuery.trim().toLowerCase();

    if (query.length < 1) {
      this.filteredMemberCandidates = [];
      this.showMemberDropdown = false;
      return;
    }

    const currentMemberAccounts = new Set(
      (this.conversation?.members || []).map(m => m.accountName.toLowerCase())
    );

    this.filteredMemberCandidates = this.allUsers.filter(u => {
      if (currentMemberAccounts.has((u.accountName || '').toLowerCase())) return false;

      const name = (u.displayName || '').toLowerCase();
      const account = (u.accountName || '').toLowerCase();
      return name.includes(query) || account.includes(query);
    }).slice(0, 8);

    this.showMemberDropdown = this.filteredMemberCandidates.length > 0;
  }

  selectMemberToAdd(user: UserData): void {
    if (this.conversation == null) return;

    this.memberSearchQuery = '';
    this.filteredMemberCandidates = [];
    this.showMemberDropdown = false;

    this.messagingApi.addMember(this.conversation.id, user.accountName).subscribe({
      next: () => {
        this.refreshMemberList();
      },
      error: (error) => {
        console.error('Failed to add member', error);
      }
    });
  }

  addMemberToConversation(): void {
    if (this.conversation == null || this.newMemberAccountName.trim().length === 0) {
      return;
    }

    const accountName = this.newMemberAccountName.trim();

    this.messagingApi.addMember(this.conversation.id, accountName).subscribe({
      next: () => {
        this.newMemberAccountName = '';
        this.refreshMemberList();
      },
      error: (error) => {
        console.error('Failed to add member', error);
      }
    });
  }

  removeMemberFromConversation(userId: number, accountName: string): void {
    if (this.conversation == null) return;

    this.messagingApi.removeMember(this.conversation.id, userId).subscribe({
      next: () => {
        if (this.conversation != null) {
          this.conversation.members = this.conversation.members.filter(
            m => m.userId !== userId
          );
          this.memberPresenceDetails.delete(userId);
        }
      },
      error: (error) => {
        console.error('Failed to remove member', error);
      }
    });
  }

  archiveCurrentConversation(): void {
    if (this.conversation == null) return;

    this.messagingApi.archiveConversation(this.conversation.id).subscribe({
      next: () => {
        this.backRequested.emit();
      },
      error: (error) => {
        console.error('Failed to archive conversation', error);
      }
    });
  }


  // ─── File Attachments ─────────────────────────────────────────────────────

  triggerFileInput(): void {
    this.fileInput?.nativeElement?.click();
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files == null) return;

    for (let i = 0; i < input.files.length; i++) {
      this.pendingAttachments.push(input.files[i]);
    }

    input.value = '';
  }

  removeAttachment(index: number): void {
    this.pendingAttachments.splice(index, 1);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    if (event.dataTransfer?.files) {
      for (let i = 0; i < event.dataTransfer.files.length; i++) {
        this.pendingAttachments.push(event.dataTransfer.files[i]);
      }
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1048576).toFixed(1) + ' MB';
  }

  getAttachmentUrl(attachmentGuid: string): string {
    return this.messagingApi.getAttachmentDownloadUrl(attachmentGuid);
  }


  //
  // Thumbnail helpers
  //

  private static IMAGE_MIMES = new Set([
    'image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/svg+xml', 'image/bmp'
  ]);

  isImageAttachment(mimeType: string): boolean {
    return ChatViewComponent.IMAGE_MIMES.has((mimeType || '').toLowerCase());
  }

  getFileIcon(mimeType: string): string {
    const mt = (mimeType || '').toLowerCase();
    if (mt === 'application/pdf') return 'fa-file-pdf';
    if (mt.includes('spreadsheet') || mt.includes('excel') || mt.includes('.sheet')) return 'fa-file-excel';
    if (mt.includes('word') || mt.includes('document') || mt.includes('.document')) return 'fa-file-word';
    if (mt.includes('presentation') || mt.includes('powerpoint')) return 'fa-file-powerpoint';
    if (mt.includes('zip') || mt.includes('compressed') || mt.includes('archive')) return 'fa-file-zipper';
    if (mt.includes('text/')) return 'fa-file-lines';
    if (mt.includes('video/')) return 'fa-file-video';
    if (mt.includes('audio/')) return 'fa-file-audio';
    return 'fa-file';
  }

  loadThumbnails(messages: MessageSummary[]): void {
    for (const msg of messages) {
      if (!msg.attachments) continue;
      for (const att of msg.attachments) {
        if (this.isImageAttachment(att.mimeType) && !this.thumbnailCache.has(att.objectGuid)) {
          this.messagingApi.downloadAttachmentBlob(att.objectGuid).subscribe({
            next: (blob) => {
              const url = URL.createObjectURL(blob);
              this.thumbnailCache.set(att.objectGuid, url);
              this.cdr.detectChanges();
            },
            error: () => { /* silently skip failed thumbnails */ }
          });
        }
      }
    }
  }

  downloadAttachment(objectGuid: string, fileName: string): void {
    this.messagingApi.downloadAttachmentBlob(objectGuid).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Failed to download attachment', err);
      }
    });
  }

  openLightbox(objectGuid: string): void {
    const cached = this.thumbnailCache.get(objectGuid);
    if (cached) {
      this.lightboxImageUrl = cached;
    }
  }

  closeLightbox(): void {
    this.lightboxImageUrl = null;
  }

  @HostListener('document:keydown.escape')
  onEscapeLightbox(): void {
    if (this.lightboxImageUrl) {
      this.closeLightbox();
    }
  }


  // ─── Thread / Reply Support ───────────────────────────────────────────────

  openThread(msg: MessageSummary): void {
    this.threadParentMessage = msg;
    this.showConversationInfo = false;
  }

  closeThread(): void {
    this.threadParentMessage = null;
  }

  onThreadReplySent(event: { reply: MessageSummary; parentMessageId: number }): void {
    const parent = this.messages.find(m => m.id === event.parentMessageId);
    if (parent) {
      parent.replyCount = (parent.replyCount || 0) + 1;
    }
  }


  // ─── Merge-or-Add ─────────────────────────────────────────────────────────

  private mergeOrAddMessage(message: MessageSummary): void {
    const existingIndex = this.messages.findIndex(m => m.id === message.id);

    if (existingIndex >= 0) {
      const existing = this.messages[existingIndex];

      //
      // Always overwrite a pending message with the real one.
      // Also allow a richer version (with attachments) to replace a stub
      // that arrived via SignalR echo (which has attachments: []).
      //
      const incomingHasAttachments = message.attachments && message.attachments.length > 0;
      const existingLacksAttachments = !existing.attachments || existing.attachments.length === 0;

      if (existing.isPending || (incomingHasAttachments && existingLacksAttachments)) {
        this.messages[existingIndex] = message;
        this.messages = [...this.messages];

        // Load thumbnails when overwriting a stub with real attachment data
        if (incomingHasAttachments) {
          this.loadThumbnails([message]);
        }
      }
      return;
    }

    // Try to match a pending message by content (for optimistic inserts)
    const pendingIndex = this.messages.findIndex(
      m => m.isPending && m.userId === message.userId && m.message === message.message
    );
    if (pendingIndex >= 0) {
      this.messages[pendingIndex] = message;
      this.messages = [...this.messages];
    } else {
      this.messages.push(message);
      this.messages = [...this.messages];
      this.smoothScrollToBottom();
    }

    // Load thumbnails for any new image attachments on this message
    if (message.attachments && message.attachments.length > 0) {
      this.loadThumbnails([message]);
    }
  }


  // ─── SignalR Event Handlers ───────────────────────────────────────────────

  private handleNewMessage(payload: MessagePayload): void {
    if (this.conversation != null && payload.conversationId === this.conversation.id) {

      const selectedChannelId = this.selectedChannel?.id ?? null;
      const messageChannelId = payload.conversationChannelId ?? null;

      if (selectedChannelId !== messageChannelId) {
        return;
      }

      if (this.messages.some(m => m.id === payload.messageId)) {
        return;
      }

      const message: MessageSummary = {
        id: payload.messageId,
        conversationId: payload.conversationId,
        conversationChannelId: payload.conversationChannelId,
        userId: payload.userId,
        userDisplayName: payload.userDisplayName,
        message: payload.message,
        dateTimeCreated: payload.dateTimeCreated,
        parentConversationMessageId: payload.parentConversationMessageId,
        entity: null,
        entityId: null,
        externalURL: null,
        versionNumber: 1,
        acknowledged: false,
        replyCount: 0,
        attachments: [],
        reactions: []
      };

      this.mergeOrAddMessage(message);

      //
      // Re-fetch the full message (with attachment metadata) from the API.
      // Both sender and receiver need this — the SignalR payload doesn't
      // include attachment details, only a hasAttachments flag.
      // Guard with !isLoadingMessages to avoid a race with loadMessages()
      // that could clobber conversation history on fresh page loads.
      //
      if (payload.hasAttachments && !this.isLoadingMessages) {
        const channelId = this.selectedChannel?.id;
        this.messagingApi.getMessages(payload.conversationId, undefined, 1, channelId).subscribe({
          next: (latest) => {
            const fullMessage = latest.find(m => m.id === payload.messageId);
            if (fullMessage) {
              this.mergeOrAddMessage(fullMessage);
            }
          }
        });
      }

      this.messagingApi.markRead(payload.conversationId, payload.messageId).subscribe();
    }
  }

  private handleMessageEdited(payload: MessageEditPayload): void {
    const msg = this.messages.find(m => m.id === payload.messageId);
    if (msg != null) {
      msg.message = payload.newContent;
    }
  }

  private handleMessageDeleted(payload: MessageDeletePayload): void {
    this.markMessageAsDeleted(payload.messageId);
  }

  private handleTypingStarted(payload: TypingPayload): void {
    if (this.currentUserMessagingId != null && payload.userId === this.currentUserMessagingId) return;

    const existing = this.typingUsers.get(payload.userId);
    if (existing != null && existing.timeout != null) {
      clearTimeout(existing.timeout);
    }

    const timeout = setTimeout(() => {
      this.typingUsers.delete(payload.userId);
    }, 5000);

    this.typingUsers.set(payload.userId, { displayName: payload.userDisplayName, timeout });
  }

  private handleTypingStopped(payload: TypingPayload): void {
    const existing = this.typingUsers.get(payload.userId);
    if (existing != null && existing.timeout != null) {
      clearTimeout(existing.timeout);
    }
    this.typingUsers.delete(payload.userId);
  }


  // typingDisplayText removed — duplicate of typingIndicatorText (fix #7)

  private handleReactionAdded(payload: ReactionPayload): void {
    const msg = this.messages.find(m => m.id === payload.messageId);
    if (msg == null) return;

    let existing = msg.reactions.find(r => r.reaction === payload.emoji);
    if (existing != null) {
      if (!existing.userNames.includes(payload.displayName)) {
        existing.count++;
        existing.userNames.push(payload.displayName);
      }
    }
    else {
      msg.reactions.push({ reaction: payload.emoji, count: 1, userNames: [payload.displayName], currentUserReacted: false });
    }
  }

  private handleReactionRemoved(payload: ReactionPayload): void {
    const msg = this.messages.find(m => m.id === payload.messageId);
    if (msg == null) return;

    const existing = msg.reactions.find(r => r.reaction === payload.emoji);
    if (existing != null) {
      existing.count--;
      existing.userNames = existing.userNames.filter((n: string) => n !== payload.displayName);
      if (existing.count <= 0) {
        msg.reactions = msg.reactions.filter(r => r.reaction !== payload.emoji);
      }
    }
  }

  private handlePresenceChanged(payload: PresencePayload): void {
    this.presenceMap.set(payload.userId, payload.status);
  }

  private handleReadReceipt(payload: ReadReceiptPayload): void {
    if (this.currentUserMessagingId != null && payload.userId === this.currentUserMessagingId) return;

    const member = this.conversation?.members?.find(m => m.userId === payload.userId);
    const displayName = member?.displayName || `User ${payload.userId}`;

    const existing = this.readReceipts.get(payload.messageId) || [];
    if (!existing.includes(displayName)) {
      existing.push(displayName);
      this.readReceipts.set(payload.messageId, existing);
    }
  }

  getReadReceiptText(msg: MessageSummary): string | null {
    if (!this.isOwnMessage(msg)) return null;

    const readers = this.readReceipts.get(msg.id);
    if (!readers || readers.length === 0) return null;

    if (readers.length === 1) return `Seen by ${readers[0]}`;
    if (readers.length === 2) return `Seen by ${readers[0]} and ${readers[1]}`;
    return `Seen by ${readers.length} people`;
  }


  // ─── Helpers ──────────────────────────────────────────────────────────────

  /**
   * Instant scroll — used for initial conversation load (viewport hidden during scroll).
   */
  private scrollToBottom(): void {
    try {
      //
      // For CDK virtual scroll with variable-height items:
      //   1. scrollToIndex forces the viewport to render the last items
      //   2. Raw scrollTop = scrollHeight bypasses the CDK's internal height
      //      estimate (itemSize=80) which underestimates for messages with
      //      attachments, reactions, etc.  Multiple delays catch late layout.
      //
      if (this.virtualScroll && this.messages.length > 0) {
        this.virtualScroll.scrollToIndex(this.messages.length - 1);
        const el = this.virtualScroll.elementRef?.nativeElement;
        if (el) {
          setTimeout(() => { el.scrollTop = el.scrollHeight; }, 100);
          setTimeout(() => { el.scrollTop = el.scrollHeight; }, 300);
        }
      } else if (this.messageContainer?.nativeElement) {
        this.messageContainer.nativeElement.scrollTop = this.messageContainer.nativeElement.scrollHeight;
      }
    }
    catch {
      // Element may not exist yet
    }
  }

  /**
   * Smooth scroll — used when a new message arrives so it glides into view.
   * Delays briefly to let Angular render the new message before reading scrollHeight.
   */
  private smoothScrollToBottom(): void {
    try {
      if (this.virtualScroll && this.messages.length > 0) {
        // Force CDK to render the last item, then smooth-scroll after DOM update
        this.virtualScroll.scrollToIndex(this.messages.length - 1);
        const el = this.virtualScroll.elementRef?.nativeElement;
        if (el) {
          setTimeout(() => {
            el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' });
          }, 100);
        }
      } else if (this.messageContainer?.nativeElement) {
        const el = this.messageContainer.nativeElement;
        setTimeout(() => {
          el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' });
        }, 100);
      }
    }
    catch {
      // Element may not exist yet
    }
  }

  get typingUserNames(): string[] {
    return Array.from(this.typingUsers.values()).map(u => u.displayName);
  }

  get typingIndicatorText(): string {
    const names = this.typingUserNames;

    if (names.length === 0) return '';
    if (names.length === 1) return `${names[0]} is typing...`;
    if (names.length === 2) return `${names[0]} and ${names[1]} are typing...`;

    return `${names.length} people are typing...`;
  }

  isOwnMessage(message: MessageSummary): boolean {
    if (this.currentUserMessagingId != null) {
      return message.userId === this.currentUserMessagingId;
    }

    const user = this.authService.currentUser;
    if (user != null && this.conversation?.members != null) {
      const member = this.conversation.members.find(
        m => m.accountName?.toLowerCase() === (user.userName || '').toLowerCase()
      );
      if (member != null) {
        return message.userId === member.userId;
      }
    }

    return false;
  }

  shouldShowSender(msg: MessageSummary, index: number): boolean {
    if (index === 0) return true;
    if (this.isOwnMessage(msg)) return false;

    const prev = this.messages[index - 1];
    if (prev.userId !== msg.userId) return true;

    const prevDate = new Date(prev.dateTimeCreated);
    const msgDate = new Date(msg.dateTimeCreated);
    return (msgDate.getTime() - prevDate.getTime()) > 5 * 60 * 1000;
  }

  isGroupedMessage(msg: MessageSummary, index: number): boolean {
    if (index === 0) return false;
    const prev = this.messages[index - 1];
    if (prev.userId !== msg.userId) return false;

    const prevDate = new Date(prev.dateTimeCreated);
    const msgDate = new Date(msg.dateTimeCreated);
    return (msgDate.getTime() - prevDate.getTime()) <= 5 * 60 * 1000;
  }

  shouldShowDateSeparator(msg: MessageSummary, index: number): boolean {
    if (index === 0) return true;

    const prev = this.messages[index - 1];
    const prevDate = new Date(prev.dateTimeCreated);
    const msgDate = new Date(msg.dateTimeCreated);
    return prevDate.toDateString() !== msgDate.toDateString();
  }

  getDateLabel(dateStr: string): string {
    if (!dateStr) return '';

    const date = new Date(dateStr);
    const now = new Date();

    if (date.toDateString() === now.toDateString()) {
      return 'Today';
    }

    const yesterday = new Date(now);
    yesterday.setDate(yesterday.getDate() - 1);
    if (date.toDateString() === yesterday.toDateString()) {
      return 'Yesterday';
    }

    const diffDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));
    if (diffDays < 7) {
      return date.toLocaleDateString([], { weekday: 'long' });
    }

    return date.toLocaleDateString([], { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' });
  }

  formatTime(dateStr: string): string {
    if (!dateStr) return '';

    const date = new Date(dateStr);
    const now = new Date();

    if (date.toDateString() === now.toDateString()) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    const diffDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));
    if (diffDays < 7) {
      return date.toLocaleDateString([], { weekday: 'short' }) + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
  }


  // ─── Phase 5 Helpers ──────────────────────────────────────────────────────

  private refreshMemberList(): void {
    if (this.conversation == null) return;

    this.messagingApi.getConversationMembers(this.conversation.id).subscribe({
      next: (members) => {
        if (this.conversation != null) {
          this.conversation.members = members;
        }
      },
      error: (error) => {
        console.error('Failed to refresh member list', error);
      }
    });
  }

  private loadMemberPresence(): void {
    if (this.conversation == null) return;

    this.memberPresenceDetails.clear();

    this.messagingApi.getConversationPresences(this.conversation.id).subscribe({
      next: (presenceList) => {
        for (const p of presenceList) {
          this.memberPresenceDetails.set(p.userId, p);
        }
      },
      error: (error) => {
        console.warn('Failed to load member presence', error);
      }
    });
  }

  getMemberLastSeen(userId: number): string {
    const presence = this.memberPresenceDetails.get(userId);
    if (presence == null) {
      const status = this.presenceMap.get(userId);
      return status === 'online' ? 'Online' : status === 'away' ? 'Away' : 'Offline';
    }

    if (presence.status === 'online') return 'Online';
    if (presence.status === 'away') return 'Away';

    if (presence.lastSeenDateTime) {
      return 'Last seen ' + this.formatRelativeTime(presence.lastSeenDateTime);
    }

    return 'Offline';
  }

  private formatRelativeTime(isoDate: string): string {
    const now = Date.now();
    const then = new Date(isoDate).getTime();
    const diffMs = now - then;

    if (diffMs < 0) return 'just now';

    const minutes = Math.floor(diffMs / 60_000);
    if (minutes < 1) return 'just now';
    if (minutes < 60) return `${minutes}m ago`;

    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;

    const days = Math.floor(hours / 24);
    return `${days}d ago`;
  }


  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (this.reactionPickerMessageId != null && target.closest('.reaction-picker') == null && target.closest('.msg-action-react') == null) {
      this.reactionPickerMessageId = null;
    }
  }


  ngOnDestroy(): void {
    //
    // NOTE: Do NOT call leaveConversation() here.
    // The messaging component's joinAllConversationGroups() keeps ALL conversation
    // groups joined on the shared singleton SignalR connection for toast notifications.
    // Leaving here would remove the shared connection from the server-side group,
    // breaking notifications for other component instances (e.g., sidebar panel).
    //

    this.subscriptions.forEach(s => s.unsubscribe());
    this.searchSubject.complete();

    // Revoke cached blob URLs to prevent memory leaks
    this.thumbnailCache.forEach(url => URL.revokeObjectURL(url));
    this.thumbnailCache.clear();

    if (this.typingTimeout != null) {
      clearTimeout(this.typingTimeout);
    }

    this.typingUsers.forEach(u => {
      if (u.timeout != null) clearTimeout(u.timeout);
    });

    //
    // Save any unsent draft before destroying
    //
    this.saveDraftNow();

    if (this.draftSaveTimeout != null) {
      clearTimeout(this.draftSaveTimeout);
    }
  }


  // ─── Draft Persistence ──────────────────────────────────────────────────────

  private getDraftKey(conversationId: number): string {
    return `messaging_draft_${conversationId}`;
  }

  private loadDraft(conversationId: number): string {
    try {
      return localStorage.getItem(this.getDraftKey(conversationId)) || '';
    } catch {
      return '';
    }
  }

  private clearDraft(conversationId: number): void {
    try {
      localStorage.removeItem(this.getDraftKey(conversationId));
    } catch { /* ignore */ }
  }

  /**
   * Debounced draft save — called from the composer input handler.
   */
  scheduleDraftSave(): void {
    if (this.draftSaveTimeout != null) {
      clearTimeout(this.draftSaveTimeout);
    }
    this.draftSaveTimeout = setTimeout(() => this.saveDraftNow(), 500);
  }

  private saveDraftNow(): void {
    if (this.conversation == null) return;
    try {
      const text = this.messageText?.trim();
      if (text) {
        localStorage.setItem(this.getDraftKey(this.conversation.id), text);
      } else {
        localStorage.removeItem(this.getDraftKey(this.conversation.id));
      }
    } catch { /* ignore */ }
  }
}
