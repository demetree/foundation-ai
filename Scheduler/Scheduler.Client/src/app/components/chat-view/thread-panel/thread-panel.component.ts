import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { MessagingApiService, MessageSummary } from '../../../services/messaging-api.service';

@Component({
  selector: 'app-thread-panel',
  templateUrl: './thread-panel.component.html',
  styleUrls: ['./thread-panel.component.scss']
})
export class ThreadPanelComponent implements OnChanges {

  @Input() parentMessage: MessageSummary | null = null;
  @Input() conversationId: number | null = null;

  /** Emitted when thread panel is closed */
  @Output() closed = new EventEmitter<void>();

  /** Emitted when a reply is successfully sent (carries the reply + parent id for reply count update) */
  @Output() replySent = new EventEmitter<{ reply: MessageSummary; parentMessageId: number }>();

  threadMessages: MessageSummary[] = [];
  threadReplyText = '';
  isLoadingThread = false;
  threadUnreadCount = 0;


  constructor(private messagingApi: MessagingApiService) {}


  ngOnChanges(changes: SimpleChanges): void {
    if (changes['parentMessage'] && this.parentMessage) {
      this.loadThread();
    }
  }


  private loadThread(): void {
    if (!this.parentMessage) return;

    this.threadMessages = [];
    this.threadReplyText = '';
    this.isLoadingThread = true;

    this.messagingApi.getThread(this.parentMessage.id).subscribe({
      next: (replies) => {
        this.threadMessages = replies;
        this.isLoadingThread = false;

        //
        // Mark thread read position at the latest reply
        //
        if (this.conversationId && this.parentMessage && replies.length > 0) {
          const lastReply = replies[replies.length - 1];
          this.messagingApi.updateThreadReadPosition(
            this.conversationId, this.parentMessage.id, lastReply.id
          ).subscribe();
          this.threadUnreadCount = 0;
        }
      },
      error: (error) => {
        console.error('Failed to load thread', error);
        this.isLoadingThread = false;
      }
    });

    //
    // Also fetch unread count for this thread
    //
    if (this.conversationId) {
      this.messagingApi.getThreadUnreadCount(this.conversationId, this.parentMessage.id).subscribe({
        next: (result) => {
          this.threadUnreadCount = result.unreadCount;
        },
        error: () => { /* ignore */ }
      });
    }
  }


  close(): void {
    this.closed.emit();
  }


  sendReply(): void {
    if (this.threadReplyText.trim().length === 0 || !this.parentMessage || !this.conversationId) {
      return;
    }

    const text = this.threadReplyText.trim();
    this.threadReplyText = '';

    this.messagingApi.sendMessage(
      this.conversationId,
      text,
      undefined,
      undefined,
      this.parentMessage.id
    ).subscribe({
      next: (message) => {
        this.threadMessages.push(message);
        this.replySent.emit({ reply: message, parentMessageId: this.parentMessage!.id });

        //
        // Update thread read position to the new reply
        //
        if (this.conversationId && this.parentMessage) {
          this.messagingApi.updateThreadReadPosition(
            this.conversationId, this.parentMessage.id, message.id
          ).subscribe();
        }
      },
      error: (error) => {
        console.error('Failed to send thread reply', error);
        this.threadReplyText = text;
      }
    });
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
}
