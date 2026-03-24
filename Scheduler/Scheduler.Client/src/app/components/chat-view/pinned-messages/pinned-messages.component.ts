import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MessageSummary } from '../../../services/messaging-api.service';

@Component({
  selector: 'app-pinned-messages',
  templateUrl: './pinned-messages.component.html',
  styleUrls: ['./pinned-messages.component.scss']
})
export class PinnedMessagesComponent {

  @Input() pinnedMessages: MessageSummary[] = [];
  @Input() isLoading = false;

  @Output() unpinMessage = new EventEmitter<MessageSummary>();
  @Output() scrollToMessage = new EventEmitter<MessageSummary>();
  @Output() closed = new EventEmitter<void>();


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
