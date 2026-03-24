/*

   MESSAGING SIGNALR SERVICE
   =======================================================================================
   Manages the SignalR connection to the Foundation MessagingHub for real-time communication.

   Handles connection lifecycle (connect, disconnect, reconnect with exponential backoff),
   group management (conversations, user, tenant), and exposes incoming events as RxJS
   Subjects for Angular components to subscribe to.

   AI-developed as part of Foundation.Messaging Phase 2A, March 2026.

*/
import { Inject, Injectable } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';


// ─── Payload Interfaces (match server-side MessagingHub payload classes) ──────

export interface MessagePayload {
  conversationId: number;
  conversationChannelId: number | null;
  messageId: number;
  userId: number;
  userDisplayName: string;
  message: string;
  parentConversationMessageId: number | null;
  entity: string | null;
  entityId: number | null;
  dateTimeCreated: string;
  hasAttachments: boolean;
}

export interface ChannelPayload {
  conversationId: number;
  channelId: number;
  name: string;
  topic: string | null;
  isPrivate: boolean;
  isPinned: boolean;
}

export interface MessageEditPayload {
  conversationId: number;
  messageId: number;
  userId: number;
  newContent: string;
  dateTimeEdited: string;
}

export interface MessageDeletePayload {
  conversationId: number;
  messageId: number;
  userId: number;
}

export interface TypingPayload {
  conversationId: number;
  userId: number;
  userDisplayName: string;
}

export interface ReactionPayload {
  conversationId: number;
  messageId: number;
  userId: number;
  displayName: string;
  emoji: string;
}

export interface ReadReceiptPayload {
  conversationId: number;
  userId: number;
  messageId: number;
}

export interface NotificationPayload {
  message: string;
  notificationType: string;
  dateTimeCreated: string;
  priority: number;
  entity: string | null;
  entityId: number | null;
  externalURL: string | null;
}

export interface PresencePayload {
  userId: number;
  userDisplayName: string;
  status: string;
  customStatusMessage: string;
}


// ─── Call Signaling Payload Interfaces ────────────────────────────────────────

export interface CallOfferPayload {
  callId: number;
  conversationId: number;
  callType: string;
  initiatorUserId: number;
  initiatorDisplayName: string;
  providerId: string;
}

export interface CallAnswerPayload {
  callId: number;
  conversationId: number;
  userId: number;
  userDisplayName: string;
}

export interface CallDeclinePayload {
  callId: number;
  conversationId: number;
  userId: number;
  userDisplayName: string;
}

export interface CallEndPayload {
  callId: number;
  conversationId: number;
  endedByUserId: number;
  reason: string;
}

export interface IceCandidatePayload {
  callId: number;
  fromUserId: number;
  toUserId: number;
  candidate: string;
  sdpMid: string;
  sdpMLineIndex: number;
}

export interface CallMissedPayload {
  callId: number;
  conversationId: number;
  initiatorUserId: number;
  initiatorDisplayName: string;
  callType: string;
}

export interface SdpPayload {
  callId: number;
  fromUserId: number;
  toUserId: number;
  type: string;        // "offer" or "answer"
  sdp: string;
}

export interface LinkPreviewReadyPayload {
  conversationId: number;
  messageId: number;
  previews: Array<{
    id: number;
    conversationMessageId: number;
    url: string;
    title: string;
    description: string;
    imageUrl: string | null;
    siteName: string | null;
  }>;
}


@Injectable({
  providedIn: 'root'
})
export class MessagingSignalRService {

  private hubConnection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private isDestroyed = false;


  // ─── Incoming Event Subjects ──────────────────────────────────────────────

  /** Emits when a new message is received in any joined conversation group */
  public messageReceived$ = new Subject<MessagePayload>();

  /** Emits when a message is edited */
  public messageEdited$ = new Subject<MessageEditPayload>();

  /** Emits when a message is deleted */
  public messageDeleted$ = new Subject<MessageDeletePayload>();

  /** Emits when a user starts typing in a conversation */
  public typingStarted$ = new Subject<TypingPayload>();

  /** Emits when a user stops typing in a conversation */
  public typingStopped$ = new Subject<TypingPayload>();

  /** Emits when a reaction is added to a message */
  public reactionAdded$ = new Subject<ReactionPayload>();

  /** Emits when a reaction is removed from a message */
  public reactionRemoved$ = new Subject<ReactionPayload>();

  /** Emits when a message read receipt is received */
  public messageRead$ = new Subject<ReadReceiptPayload>();

  /** Emits when a real-time notification is received */
  public notificationReceived$ = new Subject<NotificationPayload>();

  /** Emits when a user's presence status changes */
  public presenceChanged$ = new Subject<PresencePayload>();

  /** Emits connection state changes (true = connected). BehaviorSubject so late subscribers get current state. */
  public connectionState$ = new BehaviorSubject<boolean>(false);

  /** Emits when a channel is created in a joined conversation */
  public channelCreated$ = new Subject<ChannelPayload>();

  /** Emits when a channel is updated */
  public channelUpdated$ = new Subject<ChannelPayload>();

  /** Emits when a channel is deleted */
  public channelDeleted$ = new Subject<ChannelPayload>();


  // ─── Call Signaling Subjects ───────────────────────────────────────────────

  /** Emits when an incoming call is received */
  public incomingCall$ = new Subject<CallOfferPayload>();

  /** Emits when a call is accepted by the callee */
  public callAccepted$ = new Subject<CallAnswerPayload>();

  /** Emits when a call is declined by the callee */
  public callDeclined$ = new Subject<CallDeclinePayload>();

  /** Emits when a call ends */
  public callEnded$ = new Subject<CallEndPayload>();

  /** Emits when an ICE candidate is received for WebRTC negotiation */
  public iceCandidate$ = new Subject<IceCandidatePayload>();

  /** Emits when a call was missed (unanswered) */
  public callMissed$ = new Subject<CallMissedPayload>();

  /** Emits when an SDP offer is received from the caller */
  public sdpOffer$ = new Subject<SdpPayload>();

  /** Emits when an SDP answer is received from the answerer */
  public sdpAnswer$ = new Subject<SdpPayload>();

  /** Emits when link previews are ready for a message (after async unfurl) */
  public linkPreviewReady$ = new Subject<LinkPreviewReadyPayload>();

  /** Emits when a scheduled message has been released by the background service */
  public scheduledMessageReleased$ = new Subject<MessagePayload>();


  constructor(
    private authService: AuthService,
    @Inject('BASE_URL') private baseUrl: string
  ) {
  }


  // ─── Connection Lifecycle ─────────────────────────────────────────────────

  /**
   * Starts the SignalR connection.  Should be called after the user has logged in.
   *
   * Automatically joins the user's personal group and tenant group.
   */
  async connect(): Promise<void> {
    this.isDestroyed = false;  // Reset in case of previous disconnect
    if (this.hubConnection != null) {
      return;
    }

    const hubUrl = this.baseUrl + 'hubs/messaging';

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.authService.accessToken || ''
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          //
          // Exponential backoff: 1s, 2s, 4s, 8s, 16s, then cap at 30s
          //
          const delay = Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          return delay;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();


    //
    // Register all incoming event handlers
    //
    this.registerEventHandlers();


    //
    // Connection state event handlers
    //
    this.hubConnection.onreconnecting(() => {
      console.log('MessagingSignalR: Reconnecting...');
      this.connectionState$.next(false);
    });

    this.hubConnection.onreconnected(() => {
      console.log('MessagingSignalR: Reconnected');
      this.reconnectAttempts = 0;
      this.connectionState$.next(true);
      this.joinDefaultGroups();
    });

    this.hubConnection.onclose(() => {
      console.log('MessagingSignalR: Connection closed');
      this.connectionState$.next(false);

      //
      // Attempt manual reconnect if not intentionally destroyed
      //
      if (this.isDestroyed == false) {
        this.attemptManualReconnect();
      }
    });

    try {
      await this.hubConnection.start();

      console.log('MessagingSignalR: Connected');

      this.reconnectAttempts = 0;
      this.connectionState$.next(true);

      await this.joinDefaultGroups();
    }
    catch (error) {
      console.error('MessagingSignalR: Failed to connect', error);
      this.connectionState$.next(false);
      this.attemptManualReconnect();
    }
  }


  /**
   * Disconnects the SignalR connection.  Should be called on logout.
   */
  async disconnect(): Promise<void> {
    this.isDestroyed = true;

    if (this.hubConnection != null) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }

    this.connectionState$.next(false);
  }


  private async attemptManualReconnect(): Promise<void> {
    if (this.isDestroyed == true || this.reconnectAttempts >= this.maxReconnectAttempts) {
      return;
    }

    this.reconnectAttempts++;

    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);
    console.log(`MessagingSignalR: Manual reconnect attempt ${this.reconnectAttempts} in ${delay}ms`);

    await new Promise(resolve => setTimeout(resolve, delay));

    if (this.isDestroyed == false) {
      this.hubConnection = null;
      await this.connect();
    }
  }


  // ─── Group Management ─────────────────────────────────────────────────────

  private async joinDefaultGroups(): Promise<void> {
    if (this.hubConnection == null || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      return;
    }

    const currentUser = this.authService.currentUser;

    if (currentUser != null) {

      //
      // Join the user's personal notification group
      //
      try {
        await this.hubConnection.invoke('JoinUser', currentUser.id);
      }
      catch (error) {
        console.error('MessagingSignalR: Failed to join user group', error);
      }

      //
      // Join the tenant group for broadcast notifications
      //
      try {
        const tenantName = currentUser.tenantName;
        if (tenantName) {
          await this.hubConnection.invoke('JoinTenant', tenantName);
        }
      }
      catch (error) {
        console.error('MessagingSignalR: Failed to join tenant group', error);
      }
    }
  }


  /**
   * Joins a conversation group to receive real-time updates for that conversation.
   * Call this when the user opens/selects a conversation.
   */
  async joinConversation(conversationId: number): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinConversation', conversationId);
    }
  }


  /**
   * Leaves a conversation group.
   * Call this when the user navigates away from a conversation.
   */
  async leaveConversation(conversationId: number): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveConversation', conversationId);
    }
  }


  // ─── Outgoing Messages ────────────────────────────────────────────────────

  /**
   * Sends a typing indicator to other users in the conversation.
   */
  async sendTypingIndicator(conversationId: number, userId: number, displayName: string): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendTypingIndicator', { conversationId, userId, userDisplayName: displayName });
    }
  }


  /**
   * Sends a stopped-typing indicator to other users in the conversation.
   */
  async sendStoppedTyping(conversationId: number, userId: number, displayName: string): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendStoppedTypingIndicator', { conversationId, userId, userDisplayName: displayName });
    }
  }


  // ─── Call Signaling Hub Methods ────────────────────────────────────────────

  /**
   * Relays an ICE candidate to another participant via the server.
   */
  async relayIceCandidate(payload: IceCandidatePayload): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('RelayIceCandidate', payload);
    }
  }


  /**
   * Relays an SDP offer to the answerer via the server.
   */
  async relaySdpOffer(payload: SdpPayload): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('RelaySdpOffer', payload);
    }
  }


  /**
   * Relays an SDP answer back to the caller via the server.
   */
  async relaySdpAnswer(payload: SdpPayload): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('RelaySdpAnswer', payload);
    }
  }


  /**
   * Initiates a call via SignalR for lower latency than REST.
   */
  async initiateCallViaHub(conversationId: number, callType: string): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('InitiateCall', { conversationId, callType });
    }
  }


  /**
   * Accepts an incoming call via SignalR.
   */
  async acceptCallViaHub(callId: number): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('AcceptCall', { callId });
    }
  }


  /**
   * Declines an incoming call via SignalR.
   */
  async declineCallViaHub(callId: number): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('DeclineCall', { callId });
    }
  }


  /**
   * Ends an active call via SignalR.
   */
  async endCallViaHub(callId: number): Promise<void> {
    if (this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('EndCall', { callId });
    }
  }


  // ─── Event Handler Registration ───────────────────────────────────────────

  private registerEventHandlers(): void {
    if (this.hubConnection == null) {
      return;
    }

    //
    // Messages
    //
    this.hubConnection.on('ReceiveMessage', (payload: MessagePayload) => {
      this.messageReceived$.next(payload);
    });

    this.hubConnection.on('ReceiveMessageEdit', (payload: MessageEditPayload) => {
      this.messageEdited$.next(payload);
    });

    this.hubConnection.on('ReceiveMessageDelete', (payload: MessageDeletePayload) => {
      this.messageDeleted$.next(payload);
    });


    //
    // Typing indicators
    //
    this.hubConnection.on('UserTyping', (payload: TypingPayload) => {
      this.typingStarted$.next(payload);
    });

    this.hubConnection.on('UserStoppedTyping', (payload: TypingPayload) => {
      this.typingStopped$.next(payload);
    });


    //
    // Reactions
    //
    this.hubConnection.on('ReactionAdded', (payload: ReactionPayload) => {
      this.reactionAdded$.next(payload);
    });

    this.hubConnection.on('ReactionRemoved', (payload: ReactionPayload) => {
      this.reactionRemoved$.next(payload);
    });


    //
    // Read receipts
    //
    this.hubConnection.on('MessageRead', (payload: ReadReceiptPayload) => {
      this.messageRead$.next(payload);
    });


    //
    // Notifications
    //
    this.hubConnection.on('ReceiveNotification', (payload: NotificationPayload) => {
      this.notificationReceived$.next(payload);
    });


    //
    // Presence
    //
    this.hubConnection.on('PresenceChanged', (payload: PresencePayload) => {
      this.presenceChanged$.next(payload);
    });


    //
    // Channel lifecycle events
    //
    this.hubConnection.on('ChannelCreated', (payload: ChannelPayload) => {
      this.channelCreated$.next(payload);
    });

    this.hubConnection.on('ChannelUpdated', (payload: ChannelPayload) => {
      this.channelUpdated$.next(payload);
    });

    this.hubConnection.on('ChannelDeleted', (payload: ChannelPayload) => {
      this.channelDeleted$.next(payload);
    });


    //
    // Call signaling events
    //
    this.hubConnection.on('IncomingCall', (payload: CallOfferPayload) => {
      this.incomingCall$.next(payload);
    });

    this.hubConnection.on('CallAccepted', (payload: CallAnswerPayload) => {
      this.callAccepted$.next(payload);
    });

    this.hubConnection.on('CallDeclined', (payload: CallDeclinePayload) => {
      this.callDeclined$.next(payload);
    });

    this.hubConnection.on('CallEnded', (payload: CallEndPayload) => {
      this.callEnded$.next(payload);
    });

    this.hubConnection.on('IceCandidate', (payload: IceCandidatePayload) => {
      this.iceCandidate$.next(payload);
    });

    this.hubConnection.on('CallMissed', (payload: CallMissedPayload) => {
      this.callMissed$.next(payload);
    });

    this.hubConnection.on('SdpOffer', (payload: SdpPayload) => {
      this.sdpOffer$.next(payload);
    });

    this.hubConnection.on('SdpAnswer', (payload: SdpPayload) => {
      this.sdpAnswer$.next(payload);
    });


    //
    // Link previews
    //
    this.hubConnection.on('LinkPreviewReady', (payload: LinkPreviewReadyPayload) => {
      this.linkPreviewReady$.next(payload);
    });


    //
    // Scheduled message released — treat as a normal incoming message
    //
    this.hubConnection.on('ScheduledMessageReleased', (payload: MessagePayload) => {
      this.scheduledMessageReleased$.next(payload);
      this.messageReceived$.next(payload);   // Also emit as a regular message so the chat view picks it up
    });
  }


  // ─── Utility ──────────────────────────────────────────────────────────────

  get isConnected(): boolean {
    return this.hubConnection != null && this.hubConnection.state === signalR.HubConnectionState.Connected;
  }


}
