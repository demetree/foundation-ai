import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MessagingApiService, ConversationSummary, MessageSummary } from '../../../services/messaging-api.service';

@Component({
  selector: 'app-forward-modal',
  templateUrl: './forward-modal.component.html',
  styleUrls: ['./forward-modal.component.scss']
})
export class ForwardModalComponent {

  @Input() message: MessageSummary | null = null;
  @Input() conversations: ConversationSummary[] = [];
  @Input() currentConversationId: number | null = null;
  @Input() currentUserId: number | null = null;

  @Output() forwarded = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  searchQuery = '';


  constructor(private messagingApi: MessagingApiService) {}


  get filteredConversations(): ConversationSummary[] {
    let convs = this.conversations.filter(c => c.id !== this.currentConversationId);
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase().trim();
      convs = convs.filter(c => {
        const name = this.getConversationName(c).toLowerCase();
        return name.includes(q);
      });
    }
    return convs;
  }


  getConversationName(conv: ConversationSummary): string {
    if (conv.name) return conv.name;
    const otherMembers = (conv.members || []).filter(m => m.userId !== this.currentUserId);
    if (otherMembers.length > 0) return otherMembers.map(m => m.displayName).join(', ');
    return 'Conversation';
  }


  forwardTo(targetConversationId: number): void {
    if (!this.message) return;

    this.messagingApi.forwardMessage(this.message.id, targetConversationId).subscribe({
      next: () => {
        this.forwarded.emit();
      },
      error: (err: any) => console.error('Failed to forward message', err)
    });
  }


  close(): void {
    this.closed.emit();
  }
}
