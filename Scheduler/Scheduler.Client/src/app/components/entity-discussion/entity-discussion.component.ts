import { Component, Input, OnInit, OnDestroy, OnChanges, SimpleChanges, ViewChild, ElementRef } from '@angular/core';
import { Subscription } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import {
  MessagingApiService, ConversationSummary, MessageSummary
} from '../../services/messaging-api.service';
import {
  MessagingSignalRService, MessagePayload
} from '../../services/messaging-signalr.service';


@Component({
  selector: 'app-entity-discussion',
  templateUrl: './entity-discussion.component.html',
  styleUrls: ['./entity-discussion.component.scss']
})
export class EntityDiscussionComponent implements OnInit, OnDestroy, OnChanges {
  @ViewChild('discussionContainer') discussionContainer!: ElementRef;

  //
  // Input bindings — host component provides entity context
  //
  @Input() entityName!: string;      // e.g. "Client", "Project", "ScheduledEvent"
  @Input() entityId!: number;
  @Input() title = 'Discussion';
  @Input() collapsed = true;

  //
  // State
  //
  conversation: ConversationSummary | null = null;
  messages: MessageSummary[] = [];
  messageText = '';
  isLoading = false;
  isExpanded = false;

  private subscriptions: Subscription[] = [];


  constructor(
    private messagingApi: MessagingApiService,
    private signalR: MessagingSignalRService,
    private authService: AuthService
  ) {}


  ngOnInit(): void {
    this.isExpanded = !this.collapsed;
    if (this.entityName && this.entityId) {
      this.loadEntityConversation();
    }
    this.subscribeToSignalR();
  }


  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['entityName'] || changes['entityId']) && !changes['entityName']?.firstChange) {
      //
      // Entity context changed — reload
      //
      this.conversation = null;
      this.messages = [];
      this.loadEntityConversation();
    }
  }


  ngOnDestroy(): void {
    //
    // NOTE: Do NOT call leaveConversation() here.
    // All groups remain joined on the shared singleton SignalR connection
    // via joinAllConversationGroups(). Leaving here would break notifications.
    //
    this.subscriptions.forEach(s => s.unsubscribe());
  }


  toggleExpanded(): void {
    this.isExpanded = !this.isExpanded;
    if (this.isExpanded && !this.conversation) {
      this.loadEntityConversation();
    }
  }


  private loadEntityConversation(): void {
    this.isLoading = true;

    this.messagingApi.getEntityConversations(this.entityName, this.entityId).subscribe({
      next: (conversations) => {
        if (conversations.length > 0) {
          this.conversation = conversations[0];
          this.signalR.joinConversation(this.conversation.id);
          this.loadMessages();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load entity conversations', error);
        this.isLoading = false;
      }
    });
  }


  private loadMessages(): void {
    if (this.conversation == null) return;

    this.messagingApi.getMessages(this.conversation.id).subscribe({
      next: (messages) => {
        this.messages = messages.reverse();
        setTimeout(() => this.scrollToBottom(), 50);
      },
      error: (error) => {
        console.error('Failed to load messages', error);
      }
    });
  }


  private subscribeToSignalR(): void {
    this.subscriptions.push(
      this.signalR.messageReceived$.subscribe((payload: MessagePayload) => {
        if (this.conversation != null && payload.conversationId === this.conversation.id) {
          const msg: MessageSummary = {
            id: payload.messageId,
            conversationId: payload.conversationId,
            conversationChannelId: payload.conversationChannelId,
            userId: payload.userId,
            userDisplayName: payload.userDisplayName,
            message: payload.message,
            dateTimeCreated: payload.dateTimeCreated,
            parentConversationMessageId: null,
            entity: null,
            entityId: null,
            externalURL: null,
            versionNumber: 1,
            acknowledged: false,
            replyCount: 0,
            attachments: [],
            reactions: []
          };
          this.messages.push(msg);
          setTimeout(() => this.scrollToBottom(), 50);
        }
      })
    );
  }


  sendMessage(): void {
    const text = this.messageText.trim();
    if (!text || !this.conversation) return;

    this.messagingApi.sendMessage(
      this.conversation.id,
      text,
      this.entityName,
      this.entityId
    ).subscribe({
      next: () => {
        this.messageText = '';
      },
      error: (error) => {
        console.error('Failed to send message', error);
      }
    });
  }


  startConversation(): void {
    this.isLoading = true;

    this.messagingApi.createEntityConversation(
      this.entityName,
      this.entityId,
      [],
      ''
    ).subscribe({
      next: (conversation) => {
        this.conversation = conversation;
        this.signalR.joinConversation(conversation.id);
        this.messages = [];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to create entity conversation', error);
        this.isLoading = false;
      }
    });
  }


  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }


  isOwnMessage(msg: MessageSummary): boolean {
    const user = this.authService.currentUser;
    return user != null && msg.userId === +user.id;
  }


  formatTime(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();
    if (isToday) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    return date.toLocaleDateString([], { month: 'short', day: 'numeric' }) + ' ' +
           date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }


  private scrollToBottom(): void {
    if (this.discussionContainer) {
      const el = this.discussionContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }
}
