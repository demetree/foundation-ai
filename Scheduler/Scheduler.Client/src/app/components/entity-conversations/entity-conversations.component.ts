/*

    ENTITY CONVERSATIONS COMPONENT
    =======================================================================================
    Reusable component for displaying and managing conversations linked to a specific entity.

    Usage:
      <app-entity-conversations entityName="Client" [entityId]="selectedClient?.id">
      </app-entity-conversations>

    This component handles:
    - Loading and displaying entity-linked conversations
    - Creating new entity conversations
    - Embedding the ChatViewComponent for inline messaging
    - Zero-state with a friendly prompt

    AI-developed as part of Foundation.Messaging entity pattern, March 2026.

*/
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import {
  MessagingApiService, ConversationSummary, PresenceSummary
} from '../../services/messaging-api.service';


@Component({
  selector: 'app-entity-conversations',
  templateUrl: './entity-conversations.component.html',
  styleUrls: ['./entity-conversations.component.scss']
})
export class EntityConversationsComponent implements OnChanges {

  @Input() entityName: string = '';
  @Input() entityId: number | null | undefined = null;

  //
  // State
  //
  conversations: ConversationSummary[] = [];
  selectedConversation: ConversationSummary | null = null;
  isLoading = false;
  isCreating = false;
  viewMode: 'list' | 'chat' | 'create' = 'list';

  //
  // Member picker state
  //
  allUsers: PresenceSummary[] = [];
  isLoadingUsers = false;
  memberSearch = '';
  selectedMembers: PresenceSummary[] = [];


  constructor(
    private messagingApi: MessagingApiService
  ) {}


  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['entityName'] || changes['entityId']) && this.entityName && this.entityId) {
      this.loadConversations();
    }
  }


  // ─── Data Loading ────────────────────────────────────────────────────────────

  loadConversations(): void {
    if (!this.entityName || !this.entityId) return;

    this.isLoading = true;

    this.messagingApi.getEntityConversations(this.entityName, this.entityId).subscribe({
      next: (conversations) => {
        this.conversations = conversations;
        this.isLoading = false;
      },
      error: () => {
        this.conversations = [];
        this.isLoading = false;
      }
    });
  }


  // ─── Actions ─────────────────────────────────────────────────────────────────

  createConversation(): void {
    if (!this.entityName || !this.entityId || this.isCreating) return;

    this.isCreating = true;

    this.messagingApi.createEntityConversation(this.entityName, this.entityId, [], '').subscribe({
      next: (conversation) => {
        this.isCreating = false;
        this.conversations.unshift(conversation);
        this.selectConversation(conversation);
      },
      error: () => {
        this.isCreating = false;
      }
    });
  }


  selectConversation(conversation: ConversationSummary): void {
    this.selectedConversation = conversation;
    this.viewMode = 'chat';
  }


  backToList(): void {
    this.selectedConversation = null;
    this.viewMode = 'list';
    this.memberSearch = '';
    this.selectedMembers = [];
    // Refresh the list to pick up any new messages / unread counts
    this.loadConversations();
  }


  // ─── Creation Flow ───────────────────────────────────────────────────────────

  startCreateFlow(): void {
    this.viewMode = 'create';
    this.memberSearch = '';
    this.selectedMembers = [];

    if (this.allUsers.length === 0) {
      this.isLoadingUsers = true;
      this.messagingApi.getAllUserPresences().subscribe({
        next: (users) => {
          this.allUsers = users;
          this.isLoadingUsers = false;
        },
        error: () => {
          this.allUsers = [];
          this.isLoadingUsers = false;
        }
      });
    }
  }


  createOpenDiscussion(): void {
    if (!this.entityName || !this.entityId || this.isCreating) return;

    this.isCreating = true;
    this.messagingApi.createEntityConversation(this.entityName, this.entityId, [], '').subscribe({
      next: (conversation) => {
        this.isCreating = false;
        this.conversations.unshift(conversation);
        this.selectConversation(conversation);
      },
      error: () => {
        this.isCreating = false;
      }
    });
  }


  createPrivateConversation(): void {
    if (!this.entityName || !this.entityId || this.isCreating || this.selectedMembers.length === 0) return;

    this.isCreating = true;
    const accountNames = this.selectedMembers.map(m => m.accountName);

    this.messagingApi.createEntityConversation(this.entityName, this.entityId, accountNames, '').subscribe({
      next: (conversation) => {
        this.isCreating = false;
        this.conversations.unshift(conversation);
        this.selectConversation(conversation);
      },
      error: () => {
        this.isCreating = false;
      }
    });
  }


  toggleMember(user: PresenceSummary): void {
    const idx = this.selectedMembers.findIndex(m => m.userId === user.userId);
    if (idx >= 0) {
      this.selectedMembers.splice(idx, 1);
    } else {
      this.selectedMembers.push(user);
    }
  }


  isMemberSelected(user: PresenceSummary): boolean {
    return this.selectedMembers.some(m => m.userId === user.userId);
  }


  get filteredUsers(): PresenceSummary[] {
    if (!this.memberSearch.trim()) return this.allUsers;
    const term = this.memberSearch.trim().toLowerCase();
    return this.allUsers.filter(u =>
      (u.displayName || '').toLowerCase().includes(term) ||
      (u.accountName || '').toLowerCase().includes(term)
    );
  }


  onConversationUpdated(updated: ConversationSummary): void {
    const index = this.conversations.findIndex(c => c.id === updated.id);
    if (index >= 0) {
      this.conversations[index] = updated;
    }
  }


  // ─── Helpers ─────────────────────────────────────────────────────────────────

  getConversationDisplayName(conversation: ConversationSummary): string {
    if (conversation.name) return conversation.name;

    //
    // For DM-style entity conversations, show member names
    //
    if (conversation.members && conversation.members.length > 0) {
      const names = conversation.members.map(m => m.displayName).filter(n => n);
      if (names.length > 0) return names.join(', ');
    }

    return `${this.entityName} Conversation`;
  }


  getTimeAgo(dateStr: string | null): string {
    if (!dateStr) return '';

    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  }
}
