/*

   CHANNELS COMPONENT
   ==========================================================================================
   Standalone panel for browsing, creating, joining, and managing top-level named channels.

   When a user selects a channel, an inline chat view is displayed within the panel using
   the reusable ChatViewComponent, so users can read and send messages without leaving
   the channels context.

   Extracted from messaging.component.ts as part of the messaging decomposition, March 2026.

*/
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import { AuthService } from '../../services/auth.service';
import { MessagingApiService, ConversationSummary } from '../../services/messaging-api.service';


@Component({
  selector: 'app-channels',
  templateUrl: './channels.component.html',
  styleUrls: ['./channels.component.scss']
})
export class ChannelsComponent implements OnInit {

  @Input() isUserLoggedIn: boolean = false;


  // ─── Panel State ───────────────────────────────────────────────────────────

  isOpen = false;


  // ─── Channel List ──────────────────────────────────────────────────────────

  channels: ConversationSummary[] = [];
  isLoading = false;


  // ─── Selected Channel (inline chat view) ──────────────────────────────────

  selectedChannel: ConversationSummary | null = null;


  // ─── Create Channel Form ───────────────────────────────────────────────────

  showCreateChannel = false;
  newChannelName = '';
  newChannelDescription = '';
  newChannelIsPublic = true;


  // ─── Browse Channels ───────────────────────────────────────────────────────

  showBrowseChannels = false;
  browseableChannels: ConversationSummary[] = [];
  isLoadingBrowseChannels = false;


  constructor(
    private messagingApi: MessagingApiService,
    private authService: AuthService
  ) { }


  ngOnInit(): void {

    //
    // Load the user's joined channels when the component initializes
    //
    if (this.isUserLoggedIn) {
      this.loadChannels();
    }
  }


  // ─── Panel Toggle ──────────────────────────────────────────────────────────

  togglePanel(): void {
    this.isOpen = !this.isOpen;

    if (this.isOpen) {
      this.loadChannels();
    }
  }

  closePanel(): void {
    this.isOpen = false;
    this.selectedChannel = null;
  }


  // ─── Channel List ──────────────────────────────────────────────────────────

  loadChannels(): void {
    this.isLoading = true;

    this.messagingApi.getConversations().subscribe({
      next: (conversations) => {
        //
        // Filter to only channel-type conversations
        //
        this.channels = conversations.filter(c => c.conversationType === 'Channel');
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load channels', error);
        this.isLoading = false;
      }
    });
  }

  selectChannel(channel: ConversationSummary): void {
    this.selectedChannel = channel;
  }

  backToChannelList(): void {
    this.selectedChannel = null;
  }

  //
  // Called when the chat view emits backRequested (e.g. after archiving).
  //
  onChatBackRequested(): void {
    this.selectedChannel = null;
    this.loadChannels();
  }

  //
  // Called when the chat view updates conversation state (e.g. read receipt).
  //
  onConversationUpdated(conversation: ConversationSummary): void {
    // Update the channel in the list
    const idx = this.channels.findIndex(c => c.id === conversation.id);
    if (idx >= 0) {
      this.channels[idx] = conversation;
    }
  }


  // ─── Create Channel ────────────────────────────────────────────────────────

  toggleCreateChannel(): void {
    this.showCreateChannel = !this.showCreateChannel;

    if (!this.showCreateChannel) {
      this.newChannelName = '';
      this.newChannelDescription = '';
      this.newChannelIsPublic = true;
    }
  }

  submitCreateChannel(): void {
    if (!this.newChannelName.trim()) return;

    this.messagingApi.createChannelConversation(
      this.newChannelName.trim(),
      this.newChannelDescription.trim(),
      this.newChannelIsPublic
    ).subscribe({
      next: (channel) => {
        this.channels = [channel, ...this.channels];
        this.showCreateChannel = false;
        this.newChannelName = '';
        this.newChannelDescription = '';
        this.newChannelIsPublic = true;

        //
        // Automatically open the newly created channel's chat view
        //
        this.selectedChannel = channel;
      },
      error: (error) => {
        console.error('Failed to create channel', error);
      }
    });
  }


  // ─── Browse Channels ───────────────────────────────────────────────────────

  openBrowseChannels(): void {
    this.showBrowseChannels = true;
    this.isLoadingBrowseChannels = true;

    this.messagingApi.browseChannels().subscribe({
      next: (channels) => {
        this.browseableChannels = channels;
        this.isLoadingBrowseChannels = false;
      },
      error: (error) => {
        console.error('Failed to browse channels', error);
        this.isLoadingBrowseChannels = false;
      }
    });
  }

  closeBrowseChannels(): void {
    this.showBrowseChannels = false;
    this.browseableChannels = [];
  }

  joinChannel(channel: ConversationSummary): void {
    this.messagingApi.joinChannel(channel.id).subscribe({
      next: (joined) => {
        //
        // Add to our channel list if not already present
        //
        const exists = this.channels.find(c => c.id === joined.id);
        if (!exists) {
          this.channels = [joined, ...this.channels];
        }

        //
        // Mark as joined in the browse list
        //
        const idx = this.browseableChannels.findIndex(c => c.id === channel.id);
        if (idx >= 0) {
          this.browseableChannels[idx] = { ...this.browseableChannels[idx], isMember: true };
        }
      },
      error: (error) => {
        console.error('Failed to join channel', error);
      }
    });
  }

  leaveChannel(channel: ConversationSummary): void {
    this.messagingApi.leaveChannel(channel.id).subscribe({
      next: () => {
        this.channels = this.channels.filter(c => c.id !== channel.id);
      },
      error: (error) => {
        console.error('Failed to leave channel', error);
      }
    });
  }


  // ─── Helpers ───────────────────────────────────────────────────────────────

  formatTime(dateStr: string | null): string {
    if (!dateStr) return '';

    const date = new Date(dateStr);
    const now = new Date();

    if (date.toDateString() === now.toDateString()) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    const diffDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));
    if (diffDays < 7) {
      return date.toLocaleDateString([], { weekday: 'short' });
    }

    return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
  }
}
