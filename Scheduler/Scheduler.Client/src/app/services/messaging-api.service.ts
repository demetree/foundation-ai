/*

   MESSAGING API SERVICE
   =======================================================================================
   Hand-crafted Angular service for calling the custom MessagingControllerBase endpoints.

   This is separate from the auto-generated CRUD data services in catalyst-data-services/,
   which talk to the generic Data Controller.  This service calls the custom messaging
   endpoints that provide higher-level operations (send message, manage conversations, etc.)

   AI-developed as part of Foundation.Messaging Phase 2A, March 2026.

   Route audit against MessagingControllerBase.cs confirmed March 2026 Phase 2B.

*/
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, catchError, map} from 'rxjs';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';


// ─── Response Interfaces ────────────────────────────────────────────────────────

export interface ConversationSummary {
  id: number;                            // backend: ConversationSummary.id
  conversationType: string;
  conversationTypeId: number | null;
  createdByUserId: number | null;
  createdByUserName: string | null;
  name: string | null;
  description: string | null;
  isPublic: boolean;
  isArchived?: boolean;
  memberCount?: number;
  lastMessagePreview: string | null;
  lastMessageDateTime: string | null;    // backend: lastMessageDateTime
  lastMessageUserName: string | null;
  lastMessageReactionCount?: number;
  unreadCount: number;
  entity: string | null;
  entityId: number | null;
  externalURL: string | null;
  priority: number;
  dateTimeCreated: string;
  members: ConversationMemberSummary[];
  isMember: boolean;
}

export interface ConversationMemberSummary {
  conversationUserId: number;
  userId: number;
  displayName: string;
  accountName: string;
  dateTimeAdded: string;
  role?: string;
}

export interface MessageSummary {
  id: number;                            // backend: MessageSummary.id
  conversationId: number;
  conversationChannelId: number | null;
  userId: number;
  userDisplayName: string;               // backend: userDisplayName
  message: string;
  dateTimeCreated: string;
  parentConversationMessageId: number | null;
  entity: string | null;
  entityId: number | null;
  externalURL: string | null;
  versionNumber: number;
  acknowledged: boolean;
  replyCount: number;
  attachments: AttachmentSummary[];
  reactions: ReactionSummary[];
  linkPreviews?: LinkPreviewSummary[];
  isDeleted?: boolean;
  isPending?: boolean;
  isFailed?: boolean;
  forwardedFromMessageId?: number;
  forwardedFromUserDisplayName?: string;
}

export interface AttachmentSummary {
  id: number;
  fileName: string;
  mimeType: string;
  contentLength: number;
  objectGuid: string;
}

export interface ReactionSummary {
  reaction: string;                      // backend: reaction (not emoji)
  count: number;
  userNames: string[];                   // backend: userNames (not userDisplayNames)
  currentUserReacted: boolean;
}

export interface LinkPreviewSummary {
  id: number;
  conversationMessageId: number;
  url: string;
  title: string;
  description: string;
  imageUrl: string | null;
  siteName: string | null;
}

export interface NotificationSummary {
  notificationDistributionId: number;
  notificationId: number;
  message: string;
  entity: string | null;
  entityId: number | null;
  externalURL: string | null;
  priority: number;
  notificationType: string;
  acknowledged: boolean;
  dateTimeCreated: string;
}

export interface PresenceSummary {
  userId: number;
  displayName: string;
  accountName: string;
  status: string;
  customStatusMessage: string;
  lastSeenDateTime: string;
  lastActivityDateTime: string;
  connectionCount: number;
}

export interface AttachmentUploadResult {
  attachmentGuid: string;
  contentSize: number;
  fileName: string;
  mimeType: string;
}

export interface ChannelSummary {
  id: number;
  conversationId: number;
  name: string;
  topic: string | null;
  isPrivate: boolean;
  isPinned: boolean;
  objectGuid: string;
  messageCount: number;
  lastMessagePreview: string | null;
}


@Injectable({
  providedIn: 'root'
})
export class MessagingApiService extends SecureEndpointBase {

  private apiBase: string;

  constructor(
    http: HttpClient,
    authService: AuthService,
    alertService: AlertService,
    @Inject('BASE_URL') baseUrl: string
  ) {
    super(http, alertService, authService);
    this.apiBase = baseUrl + 'api/Messaging';
  }


  // ─── Conversations ──────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Conversation Lifecycle

  getConversations(): Observable<ConversationSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<ConversationSummary[]>(`${this.apiBase}/Conversations?includeArchived=true`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getConversations()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/Entity/{entityName}/{entityId}")]  [HttpGet]
  //
  getEntityConversations(entityName: string, entityId: number): Observable<ConversationSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<ConversationSummary[]>(`${this.apiBase}/Conversation/Entity/${entityName}/${entityId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getEntityConversations(entityName, entityId)))
    );
  }

  getConversation(conversationId: number): Observable<ConversationSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<ConversationSummary>(`${this.apiBase}/Conversation/${conversationId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getConversation(conversationId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/DirectMessage")]  [HttpPost]
  //
  createDirectMessage(recipientAccountNames: string[], initialMessage: string): Observable<ConversationSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<ConversationSummary>(`${this.apiBase}/Conversation/DirectMessage`, {
      recipientAccountNames,
      initialMessage
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.createDirectMessage(recipientAccountNames, initialMessage)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/Entity")]  [HttpPost]
  //
  createEntityConversation(entityName: string, entityId: number, participantAccountNames: string[] = [], initialMessage: string = ''): Observable<ConversationSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<ConversationSummary>(`${this.apiBase}/Conversation/Entity`, {
      entityName,
      entityId,
      participantAccountNames,
      initialMessage
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.createEntityConversation(entityName, entityId, participantAccountNames, initialMessage)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/{conversationId}/Archive")]  [HttpPost]
  //
  archiveConversation(conversationId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Conversation/${conversationId}/Archive`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.archiveConversation(conversationId)))
    );
  }


  // Backend route: [Route("api/Messaging/Conversation/{conversationId}/Rename")]  [HttpPut]
  //
  renameConversation(conversationId: number, name: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.put(`${this.apiBase}/Conversation/${conversationId}/Rename`, { name }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.renameConversation(conversationId, name)))
    );
  }


  // ─── Messages ───────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Messaging

  //
  // Backend route: [Route("api/Messaging/Conversation/{conversationId}/Messages")]  [HttpGet]
  //                Params: page, pageSize, before, channelId
  //
  getMessages(conversationId: number, before?: string, pageSize: number = 50, channelId?: number): Observable<MessageSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();
    let params = new HttpParams().set('pageSize', pageSize.toString());

    if (before) {
      params = params.set('before', before);
    }

    if (channelId) {
      params = params.set('channelId', channelId.toString());
    }

    return this.http.get<MessageSummary[]>(`${this.apiBase}/Conversation/${conversationId}/Messages`, { headers, params }).pipe(
      catchError(error => this.handleError(error, () => this.getMessages(conversationId, before, pageSize, channelId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Message")]  [HttpPost]
  //                Body: { conversationId, message, parentMessageId?, entity?, entityId?, channelId?, attachments? }
  //
  sendMessage(conversationId: number, message: string, entity?: string, entityId?: number, parentMessageId?: number, attachments?: { attachmentGuid: string; fileName: string; mimeType: string; contentSize: number }[], channelId?: number): Observable<MessageSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<MessageSummary>(`${this.apiBase}/Message`, {
      conversationId,
      message,
      parentMessageId: parentMessageId || null,
      entity: entity || null,
      entityId: entityId || null,
      channelId: channelId || null,
      attachments: attachments || null
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.sendMessage(conversationId, message, entity, entityId, parentMessageId, attachments, channelId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Message/Forward")]  [HttpPost]
  //                Body: { sourceMessageId, targetConversationId }
  //
  forwardMessage(sourceMessageId: number, targetConversationId: number): Observable<MessageSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<MessageSummary>(`${this.apiBase}/Message/Forward`, {
      sourceMessageId,
      targetConversationId
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.forwardMessage(sourceMessageId, targetConversationId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Message/Edit")]  [HttpPut]
  //                Body: { messageId, message }
  //
  editMessage(messageId: number, message: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.put(`${this.apiBase}/Message/Edit`, { messageId, message }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.editMessage(messageId, message)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Message/{messageId}")]  [HttpDelete]
  //
  deleteMessage(messageId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.delete(`${this.apiBase}/Message/${messageId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.deleteMessage(messageId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Message/{parentMessageId}/Thread")]  [HttpGet]
  //
  getThread(parentMessageId: number): Observable<MessageSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<MessageSummary[]>(`${this.apiBase}/Message/${parentMessageId}/Thread`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getThread(parentMessageId)))
    );
  }


  // ─── Membership ─────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Membership

  //
  // Backend route: [Route("api/Messaging/Conversation/{conversationId}/Members")]  [HttpGet]
  //
  getConversationMembers(conversationId: number): Observable<any[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<any[]>(`${this.apiBase}/Conversation/${conversationId}/Members`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getConversationMembers(conversationId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/AddUser")]  [HttpPost]
  //                Body: { conversationId, accountName }
  //
  addMember(conversationId: number, accountName: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Conversation/AddUser`, { conversationId, accountName }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.addMember(conversationId, accountName)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/{conversationId}/RemoveUser/{userId}")]  [HttpPost]
  //
  removeMember(conversationId: number, userId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Conversation/${conversationId}/RemoveUser/${userId}`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.removeMember(conversationId, userId)))
    );
  }


  // ─── Read Tracking ──────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Read Tracking

  //
  // Backend route: [Route("api/Messaging/Message/{messageId}/MarkRead")]  [HttpPost]
  //
  markRead(conversationId: number, messageId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Message/${messageId}/MarkRead`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.markRead(conversationId, messageId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/MarkMessageUnread")]  [HttpPost]
  //                Body: { conversationMessageId }
  //
  markMessageUnread(conversationMessageId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/MarkMessageUnread`, { conversationMessageId }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.markMessageUnread(conversationMessageId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/{conversationId}/MarkRead")]  [HttpPost]
  //
  markConversationRead(conversationId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Conversation/${conversationId}/MarkRead`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.markConversationRead(conversationId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/UnreadCounts")]  [HttpGet]
  //
  getUnreadCounts(): Observable<{ conversationId: number; unreadCount: number }[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<{ conversationId: number; unreadCount: number }[]>(`${this.apiBase}/UnreadCounts`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getUnreadCounts()))
    );
  }


  // ─── Reactions ──────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Reactions

  //
  // Backend route: [Route("api/Messaging/Reaction")]  [HttpPost]
  //                Body: { messageId, reaction }    (note: field name is 'reaction' not 'emoji')
  //
  addReaction(messageId: number, emoji: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Reaction`, { messageId, reaction: emoji }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.addReaction(messageId, emoji)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Reaction/{reactionId}")]  [HttpDelete]
  //
  removeReaction(reactionId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.delete(`${this.apiBase}/Reaction/${reactionId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.removeReaction(reactionId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Reaction/Toggle")]  [HttpPost]
  //
  toggleReaction(messageId: number, emoji: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Reaction/Toggle`, { messageId, reaction: emoji }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.toggleReaction(messageId, emoji)))
    );
  }


  //
  // Backend route: [Route("api/Messaging/LinkPreview/Dismiss")]  [HttpPost]
  //
  dismissLinkPreview(previewId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/LinkPreview/Dismiss`, { previewId }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.dismissLinkPreview(previewId)))
    );
  }


  // ─── Notifications ──────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Notification Endpoints

  getNotifications(): Observable<NotificationSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<NotificationSummary[]>(`${this.apiBase}/Notifications`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getNotifications()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Notification")]  [HttpPost]
  //                Body: { recipientUserIds, message, entity?, entityId?, externalURL?, notificationType?, priority?, distribute? }
  //
  createNotification(recipientUserIds: number[], message: string, options?: {
    entity?: string;
    entityId?: number;
    externalURL?: string;
    notificationType?: string;
    priority?: number;
    distribute?: boolean;
  }): Observable<{ notificationId: number }> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<{ notificationId: number }>(`${this.apiBase}/Notification`, {
      recipientUserIds,
      message,
      entity: options?.entity || null,
      entityId: options?.entityId || null,
      externalURL: options?.externalURL || null,
      notificationType: options?.notificationType || null,
      priority: options?.priority ?? 10,
      distribute: options?.distribute ?? true
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.createNotification(recipientUserIds, message, options)))
    );
  }

  acknowledgeNotification(notificationDistributionId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Notification/${notificationDistributionId}/Acknowledge`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.acknowledgeNotification(notificationDistributionId)))
    );
  }

  acknowledgeAllNotifications(): Observable<{ acknowledgedCount: number }> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<{ acknowledgedCount: number }>(`${this.apiBase}/Notifications/AcknowledgeAll`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.acknowledgeAllNotifications()))
    );
  }

  getUnreadNotificationCount(): Observable<{ unreadCount: number }> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<{ unreadCount: number }>(`${this.apiBase}/Notifications/UnreadCount`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getUnreadNotificationCount()))
    );
  }

  getNotificationTypes(): Observable<{ id: number; name: string; description: string }[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<{ id: number; name: string; description: string }[]>(`${this.apiBase}/NotificationTypes`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getNotificationTypes()))
    );
  }


  // ─── Channels ───────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Channel Endpoints

  getChannels(conversationId: number): Observable<ChannelSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<ChannelSummary[]>(`${this.apiBase}/Conversation/${conversationId}/Channels`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getChannels(conversationId)))
    );
  }

  createChannel(conversationId: number, name: string, topic?: string, isPrivate: boolean = false): Observable<ChannelSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<ChannelSummary>(`${this.apiBase}/Channel`, {
      conversationId,
      name,
      topic,
      isPrivate
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.createChannel(conversationId, name, topic, isPrivate)))
    );
  }

  updateChannel(channelId: number, name: string, topic?: string, isPrivate?: boolean): Observable<ChannelSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.put<ChannelSummary>(`${this.apiBase}/Channel/${channelId}`, {
      name,
      topic,
      isPrivate
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.updateChannel(channelId, name, topic, isPrivate)))
    );
  }

  deleteChannel(channelId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.delete(`${this.apiBase}/Channel/${channelId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.deleteChannel(channelId)))
    );
  }


  // ─── Pins ─────────────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Pins

  pinMessage(messageId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Pin/${messageId}`, {}, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.pinMessage(messageId)))
    );
  }

  unpinMessage(pinId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Unpin/${pinId}`, {}, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.unpinMessage(pinId)))
    );
  }

  getPinnedMessages(conversationId: number): Observable<any[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<any[]>(
      `${this.apiBase}/Conversation/${conversationId}/Pins`, { headers }
    ).pipe(
      catchError(error => this.handleError(error, () => this.getPinnedMessages(conversationId)))
    );
  }


  // ─── Attachments ────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Attachment Endpoints

  uploadAttachment(file: File): Observable<AttachmentUploadResult> {
    // Do NOT set Content-Type here — the browser must set multipart/form-data with boundary automatically
    const headers = new HttpHeaders({
      Authorization: `Bearer ${this.authService.accessToken}`
    });
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<AttachmentUploadResult>(`${this.apiBase}/Attachment/Upload`, formData, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.uploadAttachment(file)))
    );
  }

  getAttachmentDownloadUrl(attachmentGuid: string): string {
    return `${this.apiBase}/Attachment/${attachmentGuid}`;
  }

  downloadAttachmentBlob(attachmentGuid: string): Observable<Blob> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${this.authService.accessToken}`
    });

    return this.http.get(`${this.apiBase}/Attachment/${attachmentGuid}`, {
      headers,
      responseType: 'blob'
    });
  }

  deleteAttachment(attachmentGuid: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.delete(`${this.apiBase}/Attachment/${attachmentGuid}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.deleteAttachment(attachmentGuid)))
    );
  }


  // ─── Presence ───────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Presence

  //
  // Backend route: [Route("api/Messaging/Presence/Status")]  [HttpPost]
  //                Body: { status, customStatusMessage }
  //
  updatePresence(status: string, customStatusMessage?: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Presence/Status`, { status, customStatusMessage }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.updatePresence(status, customStatusMessage)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Presence/{userId}")]  [HttpGet]
  //
  getUserPresence(userId: number): Observable<PresenceSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<PresenceSummary>(`${this.apiBase}/Presence/${userId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getUserPresence(userId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Presence/Conversation/{conversationId}")]  [HttpGet]
  //
  getConversationPresences(conversationId: number): Observable<PresenceSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<PresenceSummary[]>(`${this.apiBase}/Presence/Conversation/${conversationId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getConversationPresences(conversationId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Presence/Online")]  [HttpGet]
  //
  getOnlineUsers(): Observable<PresenceSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<PresenceSummary[]>(`${this.apiBase}/Presence/Online`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getOnlineUsers()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Presence/All")]  [HttpGet]
  //
  getAllUserPresences(): Observable<PresenceSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<PresenceSummary[]>(`${this.apiBase}/Presence/All`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getAllUserPresences()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Presence/Heartbeat")]  [HttpPost]
  //
  heartbeat(): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Presence/Heartbeat`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.heartbeat()))
    );
  }


  // ─── Search ─────────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Search

  //
  // Backend route: [Route("api/Messaging/Search")]  [HttpGet]
  //                Params: query, conversationId?, entityName?, pageSize
  //
  searchMessages(query: string, conversationId?: number): Observable<MessageSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();
    let params = new HttpParams().set('query', query);

    if (conversationId) {
      params = params.set('conversationId', conversationId.toString());
    }

    return this.http.get<MessageSummary[]>(`${this.apiBase}/Search`, { headers, params }).pipe(
      catchError(error => this.handleError(error, () => this.searchMessages(query, conversationId)))
    );
  }


  // ─── Top-Level Channels ────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Top-Level Channels

  //
  // Backend route: [Route("api/Messaging/Conversations/Channel")]  [HttpPost]
  //
  createChannelConversation(name: string, description: string, isPublic: boolean): Observable<ConversationSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<ConversationSummary>(`${this.apiBase}/Conversations/Channel`, { name, description, isPublic }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.createChannelConversation(name, description, isPublic)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversations/Channels/Browse")]  [HttpGet]
  //
  browseChannels(): Observable<ConversationSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<ConversationSummary[]>(`${this.apiBase}/Conversations/Channels/Browse`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.browseChannels()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversations/{conversationId}/Join")]  [HttpPost]
  //
  joinChannel(conversationId: number): Observable<ConversationSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<ConversationSummary>(`${this.apiBase}/Conversations/${conversationId}/Join`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.joinChannel(conversationId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversations/{conversationId}/Leave")]  [HttpPost]
  //
  leaveChannel(conversationId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Conversations/${conversationId}/Leave`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.leaveChannel(conversationId)))
    );
  }


  // ─── Notification Profile ──────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Notification Profile Endpoints

  //
  // Backend route: [Route("api/Messaging/Profile")]  [HttpGet]
  //
  getNotificationProfile(): Observable<NotificationProfile> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<NotificationProfile>(`${this.apiBase}/Profile`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getNotificationProfile()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Profile")]  [HttpPut]
  //                Body: NotificationProfileUpdate (partial — null fields not updated)
  //
  updateNotificationProfile(update: NotificationProfileUpdate): Observable<NotificationProfile> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.put<NotificationProfile>(`${this.apiBase}/Profile`, update, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.updateNotificationProfile(update)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/Profile/{userId}")]  [HttpGet]
  //
  getNotificationProfileAdmin(userId: number): Observable<NotificationProfile> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<NotificationProfile>(`${this.apiBase}/Admin/Profile/${userId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getNotificationProfileAdmin(userId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/Profile/{userId}")]  [HttpPut]
  //
  updateNotificationProfileAdmin(userId: number, update: NotificationProfileUpdate): Observable<NotificationProfile> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.put<NotificationProfile>(`${this.apiBase}/Admin/Profile/${userId}`, update, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.updateNotificationProfileAdmin(userId, update)))
    );
  }


  // ─── Admin: Flags, Audit, Delivery Logs, Metrics ──────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Admin Endpoints

  //
  // Backend route: [Route("api/Messaging/Flag")]  [HttpPost]
  //
  createFlag(conversationMessageId: number, reason: string, details?: string): Observable<MessageFlagSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<MessageFlagSummary>(`${this.apiBase}/Flag`, { conversationMessageId, reason, details }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.createFlag(conversationMessageId, reason, details)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/Flags")]  [HttpGet]
  //
  getFlags(status?: string): Observable<MessageFlagSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();
    let params = new HttpParams();
    if (status) params = params.set('status', status);

    return this.http.get<MessageFlagSummary[]>(`${this.apiBase}/Admin/Flags`, { headers, params }).pipe(
      catchError(error => this.handleError(error, () => this.getFlags(status)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/Flag/{flagId}/Resolve")]  [HttpPost]
  //
  resolveFlag(flagId: number, resolutionStatus: string, resolutionNotes?: string): Observable<MessageFlagSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<MessageFlagSummary>(`${this.apiBase}/Admin/Flag/${flagId}/Resolve`, { resolutionStatus, resolutionNotes }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.resolveFlag(flagId, resolutionStatus, resolutionNotes)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/AuditLog")]  [HttpGet]
  //
  getAuditLog(startDate?: string, endDate?: string, action?: string, maxResults: number = 100): Observable<AuditLogEntry[]> {
    const headers = this.authService.GetAuthenticationHeaders();
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (action) params = params.set('action', action);
    params = params.set('maxResults', maxResults.toString());

    return this.http.get<AuditLogEntry[]>(`${this.apiBase}/Admin/AuditLog`, { headers, params }).pipe(
      catchError(error => this.handleError(error, () => this.getAuditLog(startDate, endDate, action, maxResults)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/DeliveryLogs")]  [HttpGet]
  //
  getDeliveryLogs(userId?: number, providerId?: string, successOnly?: boolean, startDate?: string, maxResults: number = 100): Observable<DeliveryLogSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    if (providerId) params = params.set('providerId', providerId);
    if (successOnly !== undefined) params = params.set('successOnly', successOnly.toString());
    if (startDate) params = params.set('startDate', startDate);
    params = params.set('maxResults', maxResults.toString());

    return this.http.get<DeliveryLogSummary[]>(`${this.apiBase}/Admin/DeliveryLogs`, { headers, params }).pipe(
      catchError(error => this.handleError(error, () => this.getDeliveryLogs(userId, providerId, successOnly, startDate, maxResults)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/Metrics")]  [HttpGet]
  //
  getMessagingMetrics(): Observable<MessagingMetrics> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<MessagingMetrics>(`${this.apiBase}/Admin/Metrics`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getMessagingMetrics()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/Search")]  [HttpGet]
  //
  adminSearchMessages(query?: string, conversationId?: number, userId?: number, startDate?: string, endDate?: string, maxResults: number = 100): Observable<AdminMessageResult[]> {
    const headers = this.authService.GetAuthenticationHeaders();
    let params = new HttpParams();
    if (query) params = params.set('query', query);
    if (conversationId) params = params.set('conversationId', conversationId.toString());
    if (userId) params = params.set('userId', userId.toString());
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    params = params.set('maxResults', maxResults.toString());

    return this.http.get<AdminMessageResult[]>(`${this.apiBase}/Admin/Search`, { headers, params }).pipe(
      catchError(error => this.handleError(error, () => this.adminSearchMessages(query, conversationId, userId, startDate, endDate, maxResults)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Admin/Analytics")]  [HttpGet]
  //
  getMessagingAnalytics(): Observable<MessagingAnalytics> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<MessagingAnalytics>(`${this.apiBase}/Admin/Analytics`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getMessagingAnalytics()))
    );
  }


  // ─── Thread Unread Tracking ────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Thread Unread Tracking

  //
  // Backend route: [Route("api/Messaging/UpdateThreadReadPosition")]  [HttpPost]
  //                Body: { conversationId, parentMessageId, lastReadMessageId }
  //
  updateThreadReadPosition(conversationId: number, parentMessageId: number, lastReadMessageId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/UpdateThreadReadPosition`, {
      conversationId,
      parentMessageId,
      lastReadMessageId
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.updateThreadReadPosition(conversationId, parentMessageId, lastReadMessageId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/GetThreadUnreadCount")]  [HttpGet]
  //                Params: conversationId, parentMessageId
  //
  getThreadUnreadCount(conversationId: number, parentMessageId: number): Observable<{ unreadCount: number }> {
    const headers = this.authService.GetAuthenticationHeaders();
    let params = new HttpParams()
      .set('conversationId', conversationId.toString())
      .set('parentMessageId', parentMessageId.toString());

    return this.http.get<{ unreadCount: number }>(`${this.apiBase}/GetThreadUnreadCount`, { headers, params }).pipe(
      catchError(error => this.handleError(error, () => this.getThreadUnreadCount(conversationId, parentMessageId)))
    );
  }


  // ─── Channel Admin Roles ──────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Channel Admin Roles

  //
  // Backend route: [Route("api/Messaging/UpdateUserRole")]  [HttpPost]
  //                Body: { conversationId, targetUserId, newRole }
  //
  updateUserRole(conversationId: number, targetUserId: number, newRole: string): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/UpdateUserRole`, {
      conversationId,
      targetUserId,
      newRole
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.updateUserRole(conversationId, targetUserId, newRole)))
    );
  }


  // ─── Message Bookmarks ────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Message Bookmarks

  //
  // Backend route: [Route("api/Messaging/AddBookmark")]  [HttpPost]
  //                Body: { conversationMessageId, note }
  //
  addBookmark(conversationMessageId: number, note?: string): Observable<{ success: boolean; bookmarkId: number }> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<{ success: boolean; bookmarkId: number }>(`${this.apiBase}/AddBookmark`, {
      conversationMessageId,
      note: note || null
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.addBookmark(conversationMessageId, note)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/RemoveBookmark")]  [HttpPost]
  //                Body: bookmarkId (int)
  //
  removeBookmark(bookmarkId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/RemoveBookmark`, bookmarkId, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.removeBookmark(bookmarkId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/GetBookmarks")]  [HttpGet]
  //
  getBookmarks(): Observable<BookmarkSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<BookmarkSummary[]>(`${this.apiBase}/GetBookmarks`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getBookmarks()))
    );
  }


  // ─── Message Scheduling ───────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Message Scheduling

  //
  // Backend route: [Route("api/Messaging/ScheduleMessage")]  [HttpPost]
  //                Body: { conversationId, messageHtml, scheduledDateTime, parentConversationMessageId? }
  //
  scheduleMessage(conversationId: number, messageHtml: string, scheduledDateTime: string, parentConversationMessageId?: number): Observable<ScheduledMessageResult> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<ScheduledMessageResult>(`${this.apiBase}/ScheduleMessage`, {
      conversationId,
      messageHtml,
      scheduledDateTime,
      parentConversationMessageId: parentConversationMessageId || null
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.scheduleMessage(conversationId, messageHtml, scheduledDateTime, parentConversationMessageId)))
    );
  }


  // ─── Calling ──────────────────────────────────────────────────────────────
  //     Backend: MessagingControllerBase.cs  #region Call Endpoints

  //
  // Backend route: [Route("api/Messaging/Call")]  [HttpPost]
  //                Body: { conversationId, callType, providerId? }
  //
  initiateCall(conversationId: number, callType: string, providerId?: string): Observable<CallSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<CallSummary>(`${this.apiBase}/Call`, {
      conversationId,
      callType,
      providerId: providerId || null
    }, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.initiateCall(conversationId, callType, providerId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Call/{callId}/Accept")]  [HttpPost]
  //
  acceptCall(callId: number): Observable<CallSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post<CallSummary>(`${this.apiBase}/Call/${callId}/Accept`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.acceptCall(callId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Call/{callId}/Decline")]  [HttpPost]
  //
  declineCall(callId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Call/${callId}/Decline`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.declineCall(callId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Call/{callId}/End")]  [HttpPost]
  //
  endCall(callId: number): Observable<any> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.post(`${this.apiBase}/Call/${callId}/End`, null, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.endCall(callId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Call/{callId}")]  [HttpGet]
  //
  getCall(callId: number): Observable<CallSummary> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<CallSummary>(`${this.apiBase}/Call/${callId}`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getCall(callId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Call/{callId}/ConnectionInfo")]  [HttpGet]
  //
  getCallConnectionInfo(callId: number): Observable<CallConnectionInfo> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<CallConnectionInfo>(`${this.apiBase}/Call/${callId}/ConnectionInfo`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getCallConnectionInfo(callId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Conversation/{conversationId}/Calls")]  [HttpGet]
  //
  getConversationCalls(conversationId: number): Observable<CallSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<CallSummary[]>(`${this.apiBase}/Conversation/${conversationId}/Calls`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getConversationCalls(conversationId)))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Calls/Recent")]  [HttpGet]
  //
  getRecentCalls(): Observable<CallSummary[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<CallSummary[]>(`${this.apiBase}/Calls/Recent`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getRecentCalls()))
    );
  }

  //
  // Backend route: [Route("api/Messaging/Call/Providers")]  [HttpGet]
  //
  getCallProviders(): Observable<CallProviderInfo[]> {
    const headers = this.authService.GetAuthenticationHeaders();

    return this.http.get<CallProviderInfo[]>(`${this.apiBase}/Call/Providers`, { headers }).pipe(
      catchError(error => this.handleError(error, () => this.getCallProviders()))
    );
  }
}


//
// Notification Profile interfaces
//

export interface NotificationProfile {
  email: string | null;
  emailVerified: boolean;
  phone: string | null;
  phoneVerified: boolean;
  emailEnabled: boolean;
  smsEnabled: boolean;
  emailPreference: string;
  smsPreference: string;
  quietHoursEnabled: boolean;
  quietHoursStart: string;
  quietHoursEnd: string;
  timezone: string;
  pinnedConversations: number[];
  notificationPrefs: { [key: number]: string };
}

export interface NotificationProfileUpdate {
  email?: string | null;
  phone?: string | null;
  emailEnabled?: boolean | null;
  smsEnabled?: boolean | null;
  emailPreference?: string | null;
  smsPreference?: string | null;
  quietHoursEnabled?: boolean | null;
  quietHoursStart?: string | null;
  quietHoursEnd?: string | null;
  timezone?: string | null;
  pinnedConversations?: number[] | null;
  notificationPrefs?: { [key: number]: string } | null;
}


//
// Admin interfaces
//

export interface MessageFlagSummary {
  id: number;
  conversationMessageId: number;
  messagePreview: string | null;
  flaggedByUserId: number;
  flaggedByUserName: string | null;
  reason: string;
  details: string | null;
  status: string;
  reviewedByUserId: number | null;
  reviewedByUserName: string | null;
  dateTimeReviewed: string | null;
  resolutionNotes: string | null;
  dateTimeCreated: string;
}

export interface AuditLogEntry {
  id: number;
  performedByUserId: number;
  performedByUserName: string | null;
  action: string;
  entityType: string | null;
  entityId: number | null;
  details: string | null;
  dateTimeCreated: string;
}

export interface DeliveryLogSummary {
  id: number;
  userId: number;
  userName: string | null;
  providerId: string;
  destination: string | null;
  sourceType: string | null;
  success: boolean;
  errorMessage: string | null;
  dateTimeCreated: string;
}

export interface MessagingMetrics {
  totalConversations: number;
  totalMessages: number;
  totalActiveUsers: number;
  messagesToday: number;
  messagesThisWeek: number;
  openFlags: number;
  deliveryAttemptsToday: number;
  deliveryFailuresToday: number;
}

export interface AdminMessageResult {
  id: number;
  conversationId: number;
  conversationName: string;
  userId: number;
  userDisplayName: string;
  message: string;
  dateTimeCreated: string;
  parentConversationMessageId: number | null;
  entity: string | null;
  entityId: number | null;
  isDeleted: boolean;
}

export interface MessagingAnalytics {
  messagesOverTime: DailyMessageCount[];
  topUsers: UserActivity[];
  topChannels: ChannelActivity[];
  userFlows: UserMessageFlow[];
}

export interface DailyMessageCount {
  date: string;
  count: number;
}

export interface UserActivity {
  userId: number;
  displayName: string;
  messageCount: number;
  conversationCount: number;
  lastActive: string;
}

export interface ChannelActivity {
  conversationId: number;
  name: string;
  messageCount: number;
  memberCount: number;
}

export interface UserMessageFlow {
  fromUser: string;
  toUser: string;
  weight: number;
}


//
// Call interfaces
//

export interface CallSummary {
  id: number;
  tenantGuid: string;
  callType: string;
  callStatus: string;
  providerId: string;
  providerCallId: string | null;
  conversationId: number;
  initiatorUserId: number;
  initiatorDisplayName: string;
  startDateTime: string;
  answerDateTime: string | null;
  endDateTime: string | null;
  durationSeconds: number | null;
  participants: CallParticipantSummary[];
  providerCapabilities: CallProviderCapabilities | null;
}

export interface CallParticipantSummary {
  id: number;
  userId: number;
  displayName: string;
  role: string;
  status: string;
  joinedDateTime: string | null;
  leftDateTime: string | null;
}

export interface CallProviderCapabilities {
  supportsVoice: boolean;
  supportsVideo: boolean;
  supportsScreenShare: boolean;
  supportsRecording: boolean;
  supportsGroupCalls: boolean;
  maxParticipants: number;
}

export interface CallConnectionInfo {
  success: boolean;
  errorMessage: string | null;
  iceServers: IceServerConfig[];
  accessToken: string | null;
  roomUrl: string | null;
  metadata: { [key: string]: string } | null;
}

export interface IceServerConfig {
  urls: string[];
  username: string | null;
  credential: string | null;
  credentialType: string | null;
}

export interface CallProviderInfo {
  providerId: string;
  displayName: string;
  isEnabled: boolean;
  capabilities: CallProviderCapabilities;
}


//
// Bookmark interfaces
//

export interface BookmarkSummary {
  id: number;
  conversationMessageId: number;
  messagePreview: string | null;
  conversationId: number;
  conversationName: string | null;
  note: string | null;
  dateTimeCreated: string;
}


//
// Scheduling interfaces
//

export interface ScheduledMessageResult {
  success: boolean;
  messageId: number;
  scheduledDateTime: string;
}
