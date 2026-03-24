import { Component, OnInit, OnDestroy, Input, HostListener } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription, Subject, forkJoin } from 'rxjs';
import { debounceTime, switchMap, tap } from 'rxjs/operators';

import { AuthService } from '../../services/auth.service';
import {
  MessagingApiService, ConversationSummary, MessageSummary,
  PresenceSummary
} from '../../services/messaging-api.service';
import {
  MessagingSignalRService, MessagePayload, PresencePayload
} from '../../services/messaging-signalr.service';

/**
 * Simple user data interface for new-conversation user picker.
 * Populated from the messaging PresenceSummary returned by the API.
 */
export interface UserData {
  id: number;
  accountName: string;
  displayName: string;
}
import { ToastNotificationService } from '../../services/toast-notification.service';
import { UserSettingsService } from '../../services/user-settings.service';


@Component({
  selector: 'app-messaging',
  templateUrl: './messaging.component.html',
  styleUrls: ['./messaging.component.scss']
})
export class MessagingComponent implements OnInit, OnDestroy {

  //
  // Display mode: 'panel' (off-canvas), 'fullscreen' (routed), 'popout' (standalone window)
  //
  @Input() displayMode: 'panel' | 'fullscreen' | 'popout' = 'panel';

  /**
   * Wide layout: conversation list + chat view side-by-side
   * (only in fullscreen/popout AND screen width >= 768px)
   */
  get isWideLayout(): boolean {
    return this.displayMode !== 'panel' && !this.isMobileView;
  }

  get isMobileView(): boolean {
    return typeof window !== 'undefined' && window.innerWidth < 768;
  }

  //
  // Core state
  //
  isOpen = false;
  isLoading = false;
  isConnected = false;
  conversations: ConversationSummary[] = [];
  selectedConversation: ConversationSummary | null = null;
  totalUnreadCount = 0;
  private pendingConversationId: number | null = null;

  //
  // New conversation
  //
  showNewConversation = false;
  newConversationRecipient = '';
  selectedRecipients: UserData[] = [];
  allUsers: UserData[] = [];
  filteredUsers: UserData[] = [];
  isLoadingUsers = false;
  showBrowseAllUsers = false;
  browseAllFilter = '';
  browseAllFilteredUsers: UserData[] = [];
  showUserDropdown = false;

  //
  // Online Now section
  //
  onlineUsers: PresenceSummary[] = [];
  showOnlineSection = true;

  //
  // Conversation categories (pin/mute)
  //
  pinnedConversationIds: Set<number> = new Set();
  mutedConversationIds: Set<number> = new Set();
  notificationPrefs: Map<number, 'all' | 'mentions' | 'none'> = new Map();
  archivedConversationIds: Set<number> = new Set();
  showConversationContextMenu = false;
  contextMenuConversationId: number | null = null;
  contextMenuPosition = { x: 0, y: 0 };

  //
  // Search and filter
  //
  searchTerm = '';
  activeFilter: 'all' | 'unread' | 'groups' | 'dms' | 'archived' = 'all';

  //
  // Global message search
  //
  showMessageSearch = false;
  messageSearchQuery = '';
  messageSearchResults: MessageSummary[] = [];
  isSearching = false;
  private searchSubject = new Subject<string>();

  //
  // Notification settings panel
  //
  showNotificationSettings = false;

  //
  // Header overflow menu
  //
  showHeaderMenu = false;

  //
  // People panel
  //
  showPeoplePanel = false;
  allUserPresences: PresenceSummary[] = [];
  peopleFilter = '';
  isLoadingPeople = false;

  //
  // Custom status & base status
  //
  showStatusPicker = false;
  customStatusInput = '';
  currentCustomStatus = '';
  currentBaseStatus = 'Online';
  baseStatusOptions = [
    { status: 'Online', label: 'Online', icon: 'fa-solid fa-circle', cssClass: 'online' },
    { status: 'Away', label: 'Away', icon: 'fa-solid fa-moon', cssClass: 'away' },
    { status: 'Busy', label: 'Busy', icon: 'fa-solid fa-minus-circle', cssClass: 'busy' },
    { status: 'DoNotDisturb', label: 'Do Not Disturb', icon: 'fa-solid fa-bell-slash', cssClass: 'do-not-disturb' }
  ];
  statusPresets = [
    { emoji: '📅', text: 'In a meeting' },
    { emoji: '🎯', text: 'Focusing' },
    { emoji: '🏖️', text: 'Out of office' },
    { emoji: '🤒', text: 'Out sick' },
    { emoji: '🚗', text: 'Commuting' },
    { emoji: '🔇', text: 'Do not disturb' }
  ];

  //
  // Presence (for online list)
  //
  currentUserMessagingId: number | null = null;
  private heartbeatInterval: ReturnType<typeof setInterval> | null = null;

  private subscriptions: Subscription[] = [];


  constructor(
    private messagingApi: MessagingApiService,
    private signalR: MessagingSignalRService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastNotificationService,
    private userSettings: UserSettingsService
  ) {
  }


  ngOnInit(): void {
    this.subscribeToEvents();
    this.loadPinnedMutedState();

    //
    // RxJS-based search debounce (fix #10 — replaces setTimeout approach)
    //
    this.subscriptions.push(
      this.searchSubject.pipe(
        debounceTime(400),
        switchMap(query => this.messagingApi.searchMessages(query))
      ).subscribe({
        next: (results: MessageSummary[]) => {
          this.messageSearchResults = results;
          this.isSearching = false;
        },
        error: () => {
          this.messageSearchResults = [];
          this.isSearching = false;
        }
      })
    );

    //
    // Presence heartbeat — keep current user showing as "Online" (60s interval)
    //
    this.heartbeatInterval = setInterval(() => {
      this.messagingApi.heartbeat().subscribe();
    }, 60_000);
    this.messagingApi.heartbeat().subscribe();   // fire immediately on load

    //
    // Load online users for the sidebar section
    //
    this.loadOnlineUsers();

    //
    // Always load conversations so we can receive SignalR events and show
    // toast notifications even when the panel is closed.
    //
    if (this.displayMode !== 'panel') {
      this.isOpen = true;

      const convIdParam = this.route.snapshot.queryParamMap.get('conversationId');
      if (convIdParam) {
        this.pendingConversationId = parseInt(convIdParam, 10);
      }
    }

    this.loadConversations();
  }


  ngOnDestroy(): void {
    //
    // Clean up all RxJS subscriptions (SignalR events, search, toast click, etc.)
    //
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.subscriptions = [];

    //
    // Clear the presence heartbeat interval
    //
    if (this.heartbeatInterval != null) {
      clearInterval(this.heartbeatInterval);
      this.heartbeatInterval = null;
    }
  }


  // ─── Panel Toggle ───────────────────────────────────────────────────────────

  togglePanel(): void {
    this.isOpen = !this.isOpen;

    if (this.isOpen) {
      this.loadConversations();
    }
  }


  /**
   * Join SignalR groups for all loaded conversations so we receive real-time
   * events (and can show toast notifications) even when the panel is closed.
   */
  private joinAllConversationGroups(): void {
    for (const conv of this.conversations) {
      this.signalR.joinConversation(conv.id);
    }
  }

  closePanel(): void {
    this.isOpen = false;
  }

  /**
   * Public method for external callers (e.g., the channels panel via the sidebar)
   * to navigate the messaging panel to a specific conversation.
   *
   * If the panel is not open, it will be opened first.
   */
  openConversation(conversation: ConversationSummary): void {
    if (!this.isOpen) {
      this.isOpen = true;
    }

    //
    // Make sure conversations are loaded, then select the target
    //
    if (this.conversations.length === 0) {
      this.messagingApi.getConversations().subscribe({
        next: (conversations) => {
          this.conversations = conversations;
          this.populateArchivedSet();
          this.isLoading = false;
          this.selectConversation(conversation);
        }
      });
    } else {
      this.selectConversation(conversation);
    }
  }

  expandToFullscreen(): void {
    const convId = this.selectedConversation?.id;
    this.closePanel();
    this.router.navigate(['/messaging'], convId ? { queryParams: { conversationId: convId } } : {});
  }

  backToApp(): void {
    this.router.navigate(['/']);
  }

  get currentUserDisplayName(): string {
    return this.authService.currentUser?.fullName || this.authService.currentUser?.userName || 'User';
  }

  get currentUserInitials(): string {
    const name = this.currentUserDisplayName;
    const parts = name.split(' ').filter(p => p.length > 0);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return (parts[0]?.[0] || 'U').toUpperCase();
  }

  popOut(): void {
    const width = 900;
    const height = 700;
    const left = (screen.width - width) / 2;
    const top = (screen.height - height) / 2;
    window.open(
      '/messaging?popout=true',
      'messaging-popout',
      `width=${width},height=${height},left=${left},top=${top},menubar=no,toolbar=no,location=no,status=no`
    );
    this.closePanel();
  }


  // ─── SignalR ────────────────────────────────────────────────────────────────

  // Connection is now initialized by the app component at login.
  // This component only subscribes to events from the shared singleton service.

  private subscribeToEvents(): void {
    //
    // Connection state
    //
    this.subscriptions.push(
      this.signalR.connectionState$.subscribe(connected => {
        const wasDisconnected = !this.isConnected;
        this.isConnected = connected;

        // On reconnect: refresh conversations, presence, and re-register heartbeat
        if (connected && wasDisconnected && this.conversations.length > 0) {
          this.loadConversations();
          this.loadOnlineUsers();
          this.messagingApi.heartbeat().subscribe();
        }
      })
    );

    //
    // New messages — update conversation list (unread count, preview, toast)
    //
    this.subscriptions.push(
      this.signalR.messageReceived$.subscribe(payload => {
        this.handleNewMessageForList(payload);
      })
    );

    //
    // Toast click — open the messaging panel and navigate to the conversation
    //
    this.subscriptions.push(
      this.toastService.toastClicked$.subscribe(toast => {
        if (toast.type === 'message' && toast.data?.conversationId != null) {
          const conv = this.conversations.find(c => c.id === toast.data.conversationId);
          if (conv) {
            if (!this.isOpen) {
              this.isOpen = true;
            }
            this.selectConversation(conv);
          }
        }
      })
    );

    //
    // Presence — keep Online Now list in sync
    //
    this.subscriptions.push(
      this.signalR.presenceChanged$.subscribe(payload => {
        this.handlePresenceChanged(payload);
      })
    );
  }


  // ─── Conversations ──────────────────────────────────────────────────────────

  loadConversations(): void {
    this.isLoading = true;

    this.messagingApi.getConversations().subscribe({
      next: (conversations) => {
        this.conversations = conversations;
        this.populateArchivedSet();
        this.calculateTotalUnread();
        this.isLoading = false;

        //
        // Join SignalR groups for all conversations so we receive events
        // even when the panel is closed (enables toast notifications).
        //
        this.joinAllConversationGroups();

        //
        // Auto-select conversation from query params (on initial load)
        //
        if (this.pendingConversationId != null) {
          const target = this.conversations.find(c => c.id === this.pendingConversationId);
          if (target) {
            this.selectConversation(target);
          }
          this.pendingConversationId = null;
        }
      },
      error: (error) => {
        console.error('Failed to load conversations', error);
        this.isLoading = false;
      }
    });
  }

  selectConversation(conversation: ConversationSummary): void {
    this.selectedConversation = conversation;

    //
    // Resolve the current user's messaging DB userId from the conversation members.
    //
    this.currentUserMessagingId = null;
    const currentUser = this.authService.currentUser;
    if (currentUser && conversation.members) {
      const me = conversation.members.find(m =>
        m.accountName?.toLowerCase() === currentUser.userName?.toLowerCase()
      );
      if (me) {
        this.currentUserMessagingId = me.userId;
      }
    }

    //
    // Update URL query params (fullscreen/popout only) for refresh persistence
    //
    if (this.displayMode !== 'panel') {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { conversationId: conversation.id },
        queryParamsHandling: 'merge',
        replaceUrl: true
      });
    }
  }

  backToList(): void {
    this.selectedConversation = null;

    //
    // In wide layout, conversations are always visible — no need to reload.
    // In stacked mode (panel / mobile), refresh the list.
    //
    if (!this.isWideLayout) {
      this.loadConversations();
    }

    //
    // Clear conversationId from URL (fullscreen/popout only)
    //
    if (this.displayMode !== 'panel') {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { conversationId: null },
        queryParamsHandling: 'merge',
        replaceUrl: true
      });
    }
  }


  // ─── ChatView Event Handlers ──────────────────────────────────────────────

  onConversationUpdated(conversation: ConversationSummary): void {
    //
    // When the chat-view updates the conversation (e.g., unread count reset),
    // recalculate the total unread count for the list.
    //
    this.calculateTotalUnread();
  }

  onChatBackRequested(): void {
    //
    // Chat-view requests navigation back (e.g., after archiving a conversation)
    //
    this.selectedConversation = null;
    this.loadConversations();
  }


  // ─── New Conversation ───────────────────────────────────────────────────────

  openNewConversation(): void {
    this.showNewConversation = true;
    this.newConversationRecipient = '';
    this.selectedRecipients = [];
    this.filteredUsers = [];
    this.showUserDropdown = false;

    if (this.allUsers.length === 0) {
      this.isLoadingUsers = true;
      this.messagingApi.getAllUserPresences().subscribe({
        next: (presences: PresenceSummary[]) => {
          this.allUsers = presences
            .filter((p: PresenceSummary) => {
              const currentUser = this.authService.currentUser;
              return currentUser == null || p.accountName !== currentUser.userName;
            })
            .map((p: PresenceSummary) => ({
              id: p.userId,
              accountName: p.accountName,
              displayName: p.displayName
            }));
          this.isLoadingUsers = false;
        },
        error: () => {
          this.isLoadingUsers = false;
        }
      });
    }
  }

  cancelNewConversation(): void {
    this.showNewConversation = false;
    this.newConversationRecipient = '';
    this.selectedRecipients = [];
    this.showUserDropdown = false;
    this.showBrowseAllUsers = false;
    this.browseAllFilter = '';
  }

  onRecipientInput(): void {
    const query = this.newConversationRecipient.trim().toLowerCase();

    if (query.length < 1) {
      this.filteredUsers = [];
      this.showUserDropdown = false;
      return;
    }

    const selectedIds = new Set(this.selectedRecipients.map(r => r.id));

    this.filteredUsers = this.allUsers.filter(u => {
      if (selectedIds.has(u.id)) return false;

      const name = (u.displayName || '').toLowerCase();
      const account = (u.accountName || '').toLowerCase();
      return name.includes(query) || account.includes(query);
    }).slice(0, 10);

    this.showUserDropdown = this.filteredUsers.length > 0;
  }

  selectRecipient(user: UserData): void {
    if (!this.selectedRecipients.find(r => r.id === user.id)) {
      this.selectedRecipients.push(user);
    }
    this.newConversationRecipient = '';
    this.filteredUsers = [];
    this.showUserDropdown = false;
  }

  removeRecipient(user: UserData): void {
    this.selectedRecipients = this.selectedRecipients.filter(r => r.id !== user.id);
  }

  toggleBrowseAllUsers(): void {
    this.showBrowseAllUsers = !this.showBrowseAllUsers;
    if (this.showBrowseAllUsers) {
      this.browseAllFilter = '';
      this.browseAllFilteredUsers = [...this.allUsers];
    }
  }

  onBrowseAllFilterInput(): void {
    const query = this.browseAllFilter.trim().toLowerCase();
    if (!query) {
      this.browseAllFilteredUsers = [...this.allUsers];
      return;
    }
    this.browseAllFilteredUsers = this.allUsers.filter(u => {
      const name = (u.displayName || '').toLowerCase();
      const account = (u.accountName || '').toLowerCase();
      return name.includes(query) || account.includes(query);
    });
  }

  toggleBrowseUser(user: UserData): void {
    if (this.isRecipientSelected(user)) {
      this.removeRecipient(user);
    } else {
      this.selectedRecipients.push(user);
    }
  }

  isRecipientSelected(user: UserData): boolean {
    return this.selectedRecipients.some(r => r.id === user.id);
  }

  startNewConversation(): void {
    if (this.selectedRecipients.length === 0) {
      return;
    }

    const recipients = this.selectedRecipients.map(r => r.accountName);

    this.messagingApi.createDirectMessage(recipients, '').subscribe({
      next: (conversation) => {
        this.showNewConversation = false;
        this.newConversationRecipient = '';
        this.selectedRecipients = [];
        this.conversations.unshift(conversation);
        this.selectConversation(conversation);
      },
      error: (error) => {
        console.error('Failed to create conversation', error);
      }
    });
  }


  // ─── Online Now ───────────────────────────────────────────────────────────

  loadOnlineUsers(): void {
    this.messagingApi.getOnlineUsers().subscribe({
      next: (users) => {
        this.onlineUsers = users;
      },
      error: (error) => {
        console.error('Failed to load online users', error);
      }
    });
  }

  toggleOnlineSection(): void {
    this.showOnlineSection = !this.showOnlineSection;
  }


  // ─── Custom Status ──────────────────────────────────────────────────────────

  toggleStatusPicker(): void {
    this.showStatusPicker = !this.showStatusPicker;
    if (this.showStatusPicker) {
      this.customStatusInput = this.currentCustomStatus || '';
    }
  }

  setBaseStatus(status: string): void {
    this.currentBaseStatus = status;
    this.messagingApi.updatePresence(status, this.currentCustomStatus).subscribe({
      error: (err: any) => console.error('Failed to set base status', err)
    });
  }

  setCustomStatus(statusText: string): void {
    const trimmed = statusText.trim();
    if (!trimmed) return;

    this.currentCustomStatus = trimmed;
    this.customStatusInput = '';
    this.showStatusPicker = false;

    this.messagingApi.updatePresence(this.currentBaseStatus, trimmed).subscribe({
      error: (err: any) => console.error('Failed to set custom status', err)
    });
  }

  clearCustomStatus(): void {
    this.currentCustomStatus = '';
    this.customStatusInput = '';
    this.showStatusPicker = false;

    this.messagingApi.updatePresence(this.currentBaseStatus, '').subscribe({
      error: (err: any) => console.error('Failed to clear custom status', err)
    });
  }

  getPresenceClass(status: string): string {
    if (!status) return 'offline';
    switch (status.toLowerCase()) {
      case 'online': return 'online';
      case 'away': return 'away';
      case 'busy': return 'busy';
      case 'donotdisturb': return 'do-not-disturb';
      default: return 'offline';
    }
  }

  getPresenceLabel(status: string): string {
    if (!status) return 'Offline';
    switch (status.toLowerCase()) {
      case 'online': return 'Online';
      case 'away': return 'Away';
      case 'busy': return 'Busy';
      case 'donotdisturb': return 'Do Not Disturb';
      default: return 'Offline';
    }
  }


  // ─── People Panel ──────────────────────────────────────────────────────────

  openPeoplePanel(): void {
    this.showPeoplePanel = true;
    this.isLoadingPeople = true;
    this.peopleFilter = '';

    this.messagingApi.getAllUserPresences().subscribe({
      next: (presences) => {
        this.allUserPresences = presences;
        this.isLoadingPeople = false;
      },
      error: (err) => {
        console.error('Failed to load all user presences', err);
        this.isLoadingPeople = false;
      }
    });
  }

  closePeoplePanel(): void {
    this.showPeoplePanel = false;
    this.allUserPresences = [];
    this.peopleFilter = '';
  }

  get filteredPeoplePresences(): PresenceSummary[] {
    const currentUser = this.authService.currentUser;
    let filtered = this.allUserPresences;

    // Exclude self
    if (this.currentUserMessagingId != null) {
      filtered = filtered.filter(u => u.userId !== this.currentUserMessagingId);
    } else if (currentUser?.userName) {
      filtered = filtered.filter(u => u.accountName?.toLowerCase() !== currentUser.userName?.toLowerCase());
    }

    // Apply search filter
    if (this.peopleFilter.trim()) {
      const term = this.peopleFilter.trim().toLowerCase();
      filtered = filtered.filter(u =>
        (u.displayName || '').toLowerCase().includes(term) ||
        (u.accountName || '').toLowerCase().includes(term)
      );
    }

    return filtered;
  }

  get onlinePeople(): PresenceSummary[] {
    return this.filteredPeoplePresences.filter(u => u.status?.toLowerCase() === 'online');
  }

  get awayPeople(): PresenceSummary[] {
    return this.filteredPeoplePresences.filter(u => ['away', 'busy', 'donotdisturb'].includes(u.status?.toLowerCase()));
  }

  get offlinePeople(): PresenceSummary[] {
    return this.filteredPeoplePresences.filter(u => u.status?.toLowerCase() === 'offline' || !u.status);
  }

  formatLastSeen(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    if (date.getFullYear() <= 1) return 'Never connected';

    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);

    if (diffMin < 1) return 'Just now';
    if (diffMin < 60) return `${diffMin}m ago`;

    const diffHours = Math.floor(diffMin / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  get otherOnlineUsers(): PresenceSummary[] {
    const currentUser = this.authService.currentUser;

    if (this.currentUserMessagingId != null) {
      return this.onlineUsers.filter(u => u.userId !== this.currentUserMessagingId);
    }

    if (currentUser?.userName) {
      return this.onlineUsers.filter(u =>
        u.accountName?.toLowerCase() !== currentUser.userName?.toLowerCase()
      );
    }

    return this.onlineUsers;
  }

  startDirectMessage(user: PresenceSummary): void {
    const existing = this.conversations.find(c =>
      c.members && c.members.length === 2 &&
      c.members.some(m => m.userId === user.userId)
    );

    if (existing) {
      this.selectConversation(existing);
    } else {
      if (user.accountName) {
        this.messagingApi.createDirectMessage([user.accountName], '').subscribe({
          next: (conv: ConversationSummary) => {
            this.conversations.unshift(conv);
            this.selectConversation(conv);
          },
          error: (error: any) => {
            console.error('Failed to create direct message', error);
          }
        });
      }
    }
  }


  // ─── Conversation Categories (Pin/Mute) ───────────────────────────────────

  get pinnedConversations(): ConversationSummary[] {
    return this.filterConversations(this.conversations.filter(c => this.pinnedConversationIds.has(c.id)));
  }

  get unpinnedConversations(): ConversationSummary[] {
    return this.filterConversations(this.conversations.filter(c => !this.pinnedConversationIds.has(c.id)));
  }

  isConversationPinned(convId: number): boolean {
    return this.pinnedConversationIds.has(convId);
  }

  isConversationMuted(convId: number): boolean {
    return this.getNotificationPref(convId) === 'none';
  }


  getNotificationPref(convId: number): 'all' | 'mentions' | 'none' {
    return this.notificationPrefs.get(convId) || 'all';
  }


  setNotificationPref(convId: number, pref: 'all' | 'mentions' | 'none'): void {
    if (pref === 'all') {
      this.notificationPrefs.delete(convId);
      this.mutedConversationIds.delete(convId);
    } else {
      this.notificationPrefs.set(convId, pref);
      if (pref === 'none') {
        this.mutedConversationIds.add(convId);
      } else {
        this.mutedConversationIds.delete(convId);
      }
    }
    this.saveNotificationPrefs();
    this.showConversationContextMenu = false;
  }


  /**
   * Applies search term + active filter to a list of conversations.
   */
  private filterConversations(list: ConversationSummary[]): ConversationSummary[] {
    let filtered = list;

    // Text search — match name, member names, or last message preview
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      filtered = filtered.filter(c => {
        const displayName = this.getConversationDisplayName(c).toLowerCase();
        const members = (c.members || []).map(m => m.displayName?.toLowerCase() || '').join(' ');
        const preview = (c.lastMessagePreview || '').toLowerCase();
        return displayName.includes(term) || members.includes(term) || preview.includes(term);
      });
    }

    // Type filter
    switch (this.activeFilter) {
      case 'unread':
        filtered = filtered.filter(c => c.unreadCount > 0 && !this.archivedConversationIds.has(c.id));
        break;
      case 'groups':
        filtered = filtered.filter(c => ((c.members?.length || 0) >= 3 || c.conversationType === 'Channel') && !this.archivedConversationIds.has(c.id));
        break;
      case 'dms':
        filtered = filtered.filter(c => ((c.members?.length || 0) <= 2 && c.conversationType !== 'Channel') && !this.archivedConversationIds.has(c.id));
        break;
      case 'archived':
        filtered = filtered.filter(c => this.archivedConversationIds.has(c.id));
        break;
      default: // 'all'
        filtered = filtered.filter(c => !this.archivedConversationIds.has(c.id));
        break;
    }

    return filtered;
  }


  toggleFilter(filter: 'all' | 'unread' | 'groups' | 'dms' | 'archived'): void {
    this.activeFilter = this.activeFilter === filter ? 'all' : filter;
  }


  clearSearch(): void {
    this.searchTerm = '';
  }


  // ─── Message Search ────────────────────────────────────────────────────────

  openMessageSearch(): void {
    this.showMessageSearch = true;
    this.messageSearchQuery = '';
    this.messageSearchResults = [];
    this.showNewConversation = false;
    setTimeout(() => {
      const el = document.getElementById('message-search-input');
      if (el) el.focus();
    }, 50);
  }


  closeMessageSearch(): void {
    this.showMessageSearch = false;
    this.messageSearchQuery = '';
    this.messageSearchResults = [];
    this.isSearching = false;
  }


  onMessageSearchInput(): void {
    const query = this.messageSearchQuery.trim();
    if (query.length < 2) {
      this.messageSearchResults = [];
      this.isSearching = false;
      return;
    }

    this.isSearching = true;
    this.searchSubject.next(query);
  }


  navigateToSearchResult(result: MessageSummary): void {
    const conv = this.conversations.find(c => c.id === result.conversationId);
    if (conv) {
      this.selectConversation(conv);
      this.showMessageSearch = false;
    }
  }


  getConversationNameById(convId: number): string {
    const conv = this.conversations.find(c => c.id === convId);
    return conv ? this.getConversationDisplayName(conv) : `Conversation #${convId}`;
  }





  highlightSearchMatch(text: string): string {
    if (!text || !this.messageSearchQuery?.trim()) return text || '';
    const query = this.messageSearchQuery.trim();

    // Escape HTML entities in the text first (fix #5 — XSS prevention)
    const escaped = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');

    const queryEscaped = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return escaped.replace(new RegExp(`(${queryEscaped})`, 'gi'), '<mark>$1</mark>');
  }

  togglePinConversation(convId: number): void {
    if (this.pinnedConversationIds.has(convId)) {
      this.pinnedConversationIds.delete(convId);
    } else {
      this.pinnedConversationIds.add(convId);
    }
    this.savePinnedState();
    this.showConversationContextMenu = false;
  }

  toggleMuteConversation(convId: number): void {
    const current = this.getNotificationPref(convId);
    this.setNotificationPref(convId, current === 'none' ? 'all' : 'none');
  }


  private populateArchivedSet(): void {
    this.archivedConversationIds.clear();
    for (const conv of this.conversations) {
      if (conv.isArchived) {
        this.archivedConversationIds.add(conv.id);
      }
    }
  }

  archiveConversation(convId: number): void {
    this.messagingApi.archiveConversation(convId).subscribe({
      next: () => {
        this.archivedConversationIds.add(convId);
        if (this.selectedConversation?.id === convId) {
          this.selectedConversation = null;
        }
      },
      error: (err: any) => console.error('Failed to archive conversation', err)
    });
    this.showConversationContextMenu = false;
  }


  renamingConversationId: number | null = null;
  renameText = '';

  renameConversation(convId: number): void {
    this.showConversationContextMenu = false;
    const conv = this.conversations.find(c => c.id === convId);
    this.renameText = conv ? this.getConversationDisplayName(conv) : '';
    this.renamingConversationId = convId;

    // Focus the input after Angular renders it
    setTimeout(() => {
      const el = document.getElementById('rename-input-' + convId);
      if (el) { el.focus(); (el as HTMLInputElement).select(); }
    }, 50);
  }

  confirmRename(convId: number): void {
    const conv = this.conversations.find(c => c.id === convId);
    const currentName = conv ? this.getConversationDisplayName(conv) : '';

    if (this.renameText.trim() && this.renameText.trim() !== currentName) {
      this.messagingApi.renameConversation(convId, this.renameText.trim()).subscribe({
        next: () => {
          if (conv) { conv.name = this.renameText.trim(); }
        },
        error: (err: any) => console.error('Failed to rename conversation', err)
      });
    }

    this.renamingConversationId = null;
    this.renameText = '';
  }

  cancelRename(): void {
    this.renamingConversationId = null;
    this.renameText = '';
  }

  openConversationContextMenu(event: MouseEvent, convId: number): void {
    event.preventDefault();
    event.stopPropagation();
    this.contextMenuConversationId = convId;
    this.contextMenuPosition = {
      x: Math.min(event.clientX, window.innerWidth - 180),
      y: Math.min(event.clientY, window.innerHeight - 100)
    };
    this.showConversationContextMenu = true;
  }

  closeConversationContextMenu(): void {
    this.showConversationContextMenu = false;
    this.contextMenuConversationId = null;
  }

  private savePinnedState(): void {
    try {
      const value = JSON.stringify(Array.from(this.pinnedConversationIds));
      this.userSettings.setStringSetting('messaging_pinned', value).subscribe();
    } catch { /* ignore */ }
  }

  private saveNotificationPrefs(): void {
    try {
      const obj: Record<string, string> = {};
      this.notificationPrefs.forEach((v, k) => obj[k.toString()] = v);
      this.userSettings.setStringSetting('messaging_notification_prefs', JSON.stringify(obj)).subscribe();
    } catch { /* ignore */ }
  }

  private loadPinnedMutedState(): void {
    forkJoin([
      this.userSettings.getStringSetting('messaging_pinned'),
      this.userSettings.getStringSetting('messaging_notification_prefs')
    ]).subscribe(([pinned, prefs]) => {
      try {
        if (pinned) {
          this.pinnedConversationIds = new Set(JSON.parse(pinned));
        }
        if (prefs) {
          const obj = JSON.parse(prefs);
          for (const [k, v] of Object.entries(obj)) {
            this.notificationPrefs.set(Number(k), v as 'all' | 'mentions' | 'none');
            if (v === 'none') {
              this.mutedConversationIds.add(Number(k));
            }
          }
        }
      } catch { /* ignore corrupt data */ }
    });
  }


  // ─── SignalR Event Handlers (list-level only) ─────────────────────────────

  private handleNewMessageForList(payload: MessagePayload): void {
    const isOwn = this.currentUserMessagingId != null && payload.userId === this.currentUserMessagingId;
    const isSelectedConversation = this.selectedConversation != null
      && payload.conversationId === this.selectedConversation.id;

    //
    // Update conversation preview / unread for messages NOT in the currently selected conversation.
    // The chat-view handles rendering messages for the selected conversation.
    //
    if (!isSelectedConversation) {
      const conv = this.conversations.find(c => c.id === payload.conversationId);
      if (conv != null) {
        conv.unreadCount = (conv.unreadCount || 0) + 1;
        conv.lastMessagePreview = payload.message
          ? payload.message.replace(/<[^>]+>/g, ' ').replace(/&[^;]+;/g, ' ').replace(/\s+/g, ' ').trim().substring(0, 100)
          : null;
        conv.lastMessageDateTime = payload.dateTimeCreated;
        this.calculateTotalUnread();
      }
    }

    //
    // Show toast notification for messages from other users, unless
    // the user is actively viewing that conversation (panel open or fullscreen).
    //
    if (!isOwn) {
      const isViewingConversation = isSelectedConversation && this.isOpen;

      //
      // When displayMode is 'panel', defer to the fullscreen instance
      // if the user has navigated to /messaging — that instance handles
      // its own toast suppression and we don't want duplicates.
      //
      const fullscreenIsActive = this.displayMode === 'panel'
        && this.router.url.startsWith('/messaging');

      if (!isViewingConversation && !fullscreenIsActive) {
        const stripped = payload.message.replace(/<[^>]+>/g, ' ').replace(/&[^;]+;/g, ' ').replace(/\s+/g, ' ').trim();
        const preview = stripped.length > 80 ? stripped.substring(0, 80) + '...' : stripped;
        this.toastService.showMessage(payload.userDisplayName, preview, { conversationId: payload.conversationId });
      }
    }
  }

  private handlePresenceChanged(payload: PresencePayload): void {
    //
    // Keep the Online Now list in sync
    //
    const isOnline = payload.status.toLowerCase() !== 'offline';
    const existingIndex = this.onlineUsers.findIndex(u => u.userId === payload.userId);

    if (isOnline && existingIndex === -1) {
      this.onlineUsers.push({
        userId: payload.userId,
        displayName: payload.userDisplayName,
        accountName: '',
        status: payload.status,
        customStatusMessage: payload.customStatusMessage || '',
        lastSeenDateTime: new Date().toISOString(),
        lastActivityDateTime: new Date().toISOString(),
        connectionCount: 1
      });
      this.onlineUsers.sort((a, b) => (a.displayName || '').localeCompare(b.displayName || ''));

      //
      // Show a toast notification when another user comes online
      // Skip for self (don't alert yourself about your own login)
      //
      const isSelf = this.currentUserMessagingId != null && payload.userId === this.currentUserMessagingId;
      if (!isSelf && payload.status.toLowerCase() === 'online') {
        this.toastService.show(`${payload.userDisplayName} is now online`, { type: 'info', icon: 'fa-solid fa-circle-check', duration: 4000 });
      }
    } else if (isOnline && existingIndex >= 0) {
      this.onlineUsers[existingIndex].status = payload.status;
      this.onlineUsers[existingIndex].customStatusMessage = payload.customStatusMessage || this.onlineUsers[existingIndex].customStatusMessage;
    } else if (!isOnline && existingIndex >= 0) {
      this.onlineUsers.splice(existingIndex, 1);
    }
  }


  // ─── Helpers ──────────────────────────────────────────────────────────────

  private calculateTotalUnread(): void {
    this.totalUnreadCount = this.conversations.reduce((sum, c) => sum + (c.unreadCount || 0), 0);
  }

  getConversationDisplayName(conversation: ConversationSummary): string {
    // For channels, return the channel name
    if (conversation.conversationType === 'Channel' && conversation.name) {
      return conversation.name;
    }

    if (conversation.members == null || conversation.members.length === 0) {
      return 'Conversation';
    }

    const currentUser = this.authService.currentUser;
    const otherMembers = conversation.members.filter(m => {
      return currentUser == null || m.accountName !== currentUser.userName;
    });

    if (otherMembers.length === 0) {
      return conversation.members[0]?.displayName || 'Conversation';
    }

    if (otherMembers.length <= 3) {
      return otherMembers.map(m => m.displayName).join(', ');
    }

    return `${otherMembers[0].displayName}, ${otherMembers[1].displayName} +${otherMembers.length - 2}`;
  }


  /**
   * Returns 1-2 character initials for the conversation avatar.
   *  - Channel → "#"
   *  - DM (1 other member) → their two initials (e.g. "NG")
   *  - Group (2+ others) → first initial of each of the first two members (e.g. "NS")
   *  - Fallback → "?"
   */
  getConversationInitials(conversation: ConversationSummary): string {
    if (conversation.conversationType === 'Channel') {
      return '#';
    }

    const currentUser = this.authService.currentUser;
    const otherMembers = (conversation.members || []).filter(m =>
      currentUser == null || m.accountName !== currentUser.userName
    );

    if (otherMembers.length === 0 && conversation.members?.length > 0) {
      // Self-conversation
      return this.extractInitials(conversation.members[0].displayName);
    }

    if (otherMembers.length === 1) {
      // DM — show the other person's initials
      return this.extractInitials(otherMembers[0].displayName);
    }

    if (otherMembers.length >= 2) {
      // Group — first initial of the first two members
      const a = otherMembers[0].displayName?.charAt(0)?.toUpperCase() || '';
      const b = otherMembers[1].displayName?.charAt(0)?.toUpperCase() || '';
      return a + b;
    }

    return '?';
  }


  /**
   * Returns a CSS background gradient whose color conveys conversation state:
   *   - Hue:        deterministic from conversation ID (consistent identity)
   *   - Saturation:  driven by unread count (0 = muted, 5+ = vivid)
   *   - Lightness:   driven by recency of last message (recent = bright, stale = dim)
   *
   * Visual result:
   *   Bright & saturated = active, needs attention
   *   Muted & dim        = dormant, nothing new
   *   Grey               = no messages yet
   */
  getAvatarGradient(conversation: ConversationSummary): string {
    //
    // Hue — deterministic identity from conversation ID
    //
    const hue = (conversation.id * 137) % 360;

    //
    // Saturation — driven by unread count
    //   0 unreads  → 30% (subdued)
    //   1 unread   → 55%
    //   3 unreads  → 70%
    //   5+ unreads → 85% (vivid, demands attention)
    //
    const unread = conversation.unreadCount || 0;
    const saturation = Math.min(30 + unread * 11, 85);

    //
    // Lightness — driven by recency of last message
    //   Last hour  → 60% (bright)
    //   Last day   → 52%
    //   Last week  → 42%
    //   Older      → 35% (dim)
    //   No messages → 30% with low saturation (grey)
    //
    let lightness: number;
    if (!conversation.lastMessageDateTime) {
      // No messages — grey
      return `linear-gradient(135deg, hsl(${hue}, 15%, 40%), hsl(${(hue + 30) % 360}, 10%, 32%))`;
    }

    const lastMsg = new Date(conversation.lastMessageDateTime);
    const hoursAgo = (Date.now() - lastMsg.getTime()) / (1000 * 60 * 60);

    if (hoursAgo < 1) {
      lightness = 60;
    } else if (hoursAgo < 24) {
      // 1h → 58%, 24h → 48%  (linear interpolation)
      lightness = 58 - (hoursAgo / 24) * 10;
    } else if (hoursAgo < 168) {
      // 1d → 48%, 7d → 38%
      lightness = 48 - ((hoursAgo - 24) / 144) * 10;
    } else {
      lightness = 35;
    }

    return `linear-gradient(135deg, hsl(${hue}, ${saturation}%, ${lightness}%), hsl(${(hue + 35) % 360}, ${Math.max(saturation - 10, 20)}%, ${lightness - 5}%))`;
  }


  /**
   * Returns true if this is a group conversation (3+ members).
   */
  isGroupConversation(conversation: ConversationSummary): boolean {
    return (conversation.members?.length || 0) >= 3;
  }


  private extractInitials(displayName: string): string {
    if (!displayName) return '?';
    const parts = displayName.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
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

  // formatSearchTime consolidated into formatTime (fix #9)

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    //
    // Close context menu if clicking outside of it
    //
    const target = event.target as HTMLElement;
    if (this.showConversationContextMenu && target.closest('.conversation-context-menu') == null) {
      this.showConversationContextMenu = false;
    }
    if (this.showHeaderMenu && target.closest('.header-overflow') == null) {
      this.showHeaderMenu = false;
    }
  }


  //
  // Notification settings panel
  //
  openNotificationSettings(): void {
    this.showNotificationSettings = true;
    this.showNewConversation = false;
    this.showMessageSearch = false;
  }

  closeNotificationSettings(): void {
    this.showNotificationSettings = false;
  }



}
