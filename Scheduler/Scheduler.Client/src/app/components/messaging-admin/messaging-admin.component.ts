import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  MessagingApiService,
  MessagingMetrics, MessageFlagSummary, AuditLogEntry, DeliveryLogSummary,
  ConversationSummary, AdminMessageResult, MessagingAnalytics, UserActivity, ChannelActivity, UserMessageFlow
} from '../../services/messaging-api.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-messaging-admin',
  templateUrl: './messaging-admin.component.html',
  styleUrls: ['./messaging-admin.component.scss']
})
export class MessagingAdminComponent implements OnInit {

  activeTab: 'overview' | 'channels' | 'messages' | 'analytics' | 'users' | 'entities' | 'moderation' | 'audit' | 'delivery' = 'overview';

  // Chart canvas refs
  @ViewChild('messagesChart') messagesChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('topUsersChart') topUsersChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('topChannelsChart') topChannelsChartRef!: ElementRef<HTMLCanvasElement>;

  //
  // Overview
  //
  metrics: MessagingMetrics | null = null;
  isLoadingMetrics = true;

  //
  // Channels (uses existing conversation APIs filtered to channel type)
  //
  channels: ConversationSummary[] = [];
  isLoadingChannels = false;
  channelsLoaded = false;

  // Channel dialog
  showChannelDialog = false;
  channelDialogMode: 'create' | 'edit' = 'create';
  editingChannel: ConversationSummary | null = null;
  channelName = '';
  channelDescription = '';
  channelIsPublic = true;
  isSavingChannel = false;
  showDeleteConfirm = false;
  deletingChannel: ConversationSummary | null = null;

  //
  // Moderation
  //
  flags: MessageFlagSummary[] = [];
  isLoadingFlags = false;
  flagsLoaded = false;
  flagStatusFilter = 'all';

  // Resolve dialog
  showResolveDialog = false;
  resolvingFlag: MessageFlagSummary | null = null;
  resolveStatus = 'dismissed';
  resolveNotes = '';
  isResolving = false;

  //
  // Audit Log
  //
  auditLog: AuditLogEntry[] = [];
  isLoadingAudit = false;
  auditLoaded = false;
  auditActionFilter = '';
  auditDateFilter = '';

  //
  // Delivery Log
  //
  deliveryLogs: DeliveryLogSummary[] = [];
  isLoadingDelivery = false;
  deliveryLoaded = false;
  deliveryProviderFilter = '';
  deliveryStatusFilter = '';

  //
  // Messages (admin search/browse)
  //
  messageSearchResults: AdminMessageResult[] = [];
  isLoadingMessages = false;
  messageSearchQuery = '';
  messageConversationFilter = '';
  messageDateFilter = '';
  private messageSearchTimeout: any = null;

  // Flag dialog
  showFlagDialog = false;
  flaggingMessage: AdminMessageResult | null = null;
  flagReason = '';
  flagDetails = '';
  isFlagging = false;

  // Tracks flagged messages with their reason/details for inline display
  flaggedMessages = new Map<number, { reason: string; details: string }>();
  expandedFlagId: number | null = null;

  readonly flagReasonOptions = [
    { value: 'harassment', label: 'Harassment or Bullying' },
    { value: 'spam', label: 'Spam or Unsolicited Content' },
    { value: 'inappropriate', label: 'Inappropriate or Offensive Content' },
    { value: 'misinformation', label: 'Misinformation' },
    { value: 'off-topic', label: 'Off-Topic or Disruptive' },
    { value: 'confidential', label: 'Confidential Information Shared' },
    { value: 'other', label: 'Other' }
  ];

  //
  // Analytics
  //
  analytics: MessagingAnalytics | null = null;
  isLoadingAnalytics = false;
  analyticsLoaded = false;
  sankeyPaths: { d: string; color: string; opacity: number; fromLabel: string; toLabel: string; weight: number; fromY: number; toY: number }[] = [];
  sankeyLabels: { text: string; x: number; y: number; align: string }[] = [];
  private chartInstances: Chart[] = [];

  //
  // Users (from analytics.topUsers)
  //
  userSearchFilter = '';

  //
  // Entities
  //
  entityTypeFilter = '';
  entityIdFilter: number | null = null;
  entityResults: AdminMessageResult[] = [];
  isLoadingEntities = false;
  entityConversations: ConversationSummary[] = [];


  constructor(
    private messagingApi: MessagingApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMetrics();
    this.loadAnalytics();
  }


  // ─── Tab Switch ──────────────────────────────────────────────────────────

  switchTab(tab: typeof this.activeTab): void {
    this.activeTab = tab;

    // Lazy load data on first visit
    switch (tab) {
      case 'overview':
        if (this.analyticsLoaded) {
          setTimeout(() => this.renderCharts(), 100);
        }
        break;
      case 'channels':
        if (!this.channelsLoaded) this.loadChannels();
        break;
      case 'messages':
        if (this.messageSearchResults.length === 0) this.loadMessages();
        break;
      case 'users':
        if (!this.analyticsLoaded) this.loadAnalytics();
        break;
      case 'entities':
        if (this.entityConversations.length === 0) this.loadEntityConversations();
        break;
      case 'moderation':
        if (!this.flagsLoaded) this.loadFlags();
        break;
      case 'audit':
        if (!this.auditLoaded) this.loadAuditLog();
        break;
      case 'delivery':
        if (!this.deliveryLoaded) this.loadDeliveryLogs();
        break;
    }
  }


  // ─── Overview ────────────────────────────────────────────────────────────

  loadMetrics(): void {
    this.isLoadingMetrics = true;
    this.messagingApi.getMessagingMetrics().subscribe({
      next: (data) => {
        this.metrics = data;
        this.isLoadingMetrics = false;
      },
      error: () => {
        this.isLoadingMetrics = false;
      }
    });
  }


  // ─── Channels ────────────────────────────────────────────────────────────

  loadChannels(): void {
    this.isLoadingChannels = true;
    // browseChannels() returns ALL channels for the tenant (not just ones the user is a member of)
    this.messagingApi.browseChannels().subscribe({
      next: (data: ConversationSummary[]) => {
        this.channels = data;
        this.isLoadingChannels = false;
        this.channelsLoaded = true;
      },
      error: () => {
        this.isLoadingChannels = false;
        this.channelsLoaded = true;
      }
    });
  }

  openCreateChannel(): void {
    this.channelDialogMode = 'create';
    this.editingChannel = null;
    this.channelName = '';
    this.channelDescription = '';
    this.channelIsPublic = true;
    this.showChannelDialog = true;
  }

  openEditChannel(ch: ConversationSummary): void {
    this.channelDialogMode = 'edit';
    this.editingChannel = ch;
    this.channelName = ch.name || '';
    this.channelDescription = ch.description || '';
    this.channelIsPublic = ch.conversationType !== 'Private';
    this.showChannelDialog = true;
  }

  closeChannelDialog(): void {
    this.showChannelDialog = false;
    this.editingChannel = null;
  }

  saveChannel(): void {
    if (!this.channelName.trim()) return;
    this.isSavingChannel = true;

    if (this.channelDialogMode === 'create') {
      this.messagingApi.createChannelConversation(this.channelName, this.channelDescription, this.channelIsPublic).subscribe({
        next: () => {
          this.isSavingChannel = false;
          this.closeChannelDialog();
          this.loadChannels();
        },
        error: () => { this.isSavingChannel = false; }
      });
    } else if (this.editingChannel) {
      this.messagingApi.updateChannel(this.editingChannel.id, this.channelName, this.channelDescription, !this.channelIsPublic).subscribe({
        next: () => {
          this.isSavingChannel = false;
          this.closeChannelDialog();
          this.loadChannels();
        },
        error: () => { this.isSavingChannel = false; }
      });
    }
  }

  confirmDeleteChannel(ch: ConversationSummary): void {
    this.deletingChannel = ch;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.deletingChannel = null;
  }

  executeDeleteChannel(): void {
    if (!this.deletingChannel) return;
    this.messagingApi.deleteChannel(this.deletingChannel.id).subscribe({
      next: () => {
        this.showDeleteConfirm = false;
        this.deletingChannel = null;
        this.loadChannels();
      },
      error: () => {
        this.showDeleteConfirm = false;
      }
    });
  }


  // ─── Moderation ──────────────────────────────────────────────────────────

  loadFlags(): void {
    this.isLoadingFlags = true;

    const status = this.flagStatusFilter === 'all' ? undefined : this.flagStatusFilter;
    this.messagingApi.getFlags(status).subscribe({
      next: (data) => {
        this.flags = data;
        this.isLoadingFlags = false;
        this.flagsLoaded = true;
      },
      error: () => {
        this.isLoadingFlags = false;
        this.flagsLoaded = true;
      }
    });
  }

  onFlagFilterChange(): void {
    this.loadFlags();
  }

  openResolveDialog(flag: MessageFlagSummary): void {
    this.resolvingFlag = flag;
    this.resolveStatus = 'dismissed';
    this.resolveNotes = '';
    this.showResolveDialog = true;
  }

  closeResolveDialog(): void {
    this.showResolveDialog = false;
    this.resolvingFlag = null;
  }

  submitResolve(): void {
    if (!this.resolvingFlag) return;
    this.isResolving = true;

    this.messagingApi.resolveFlag(this.resolvingFlag.id, this.resolveStatus, this.resolveNotes).subscribe({
      next: () => {
        this.isResolving = false;
        this.closeResolveDialog();
        this.loadFlags();
      },
      error: () => {
        this.isResolving = false;
      }
    });
  }

  // Edit flag (update reason/details/status)
  showEditFlagDialog = false;
  editingFlag: MessageFlagSummary | null = null;
  editFlagReason = '';
  editFlagDetails = '';
  editFlagStatus = '';
  isSavingFlag = false;

  openEditFlag(flag: MessageFlagSummary): void {
    this.editingFlag = flag;
    this.editFlagReason = flag.reason || '';
    this.editFlagDetails = flag.details || '';
    this.editFlagStatus = flag.status || 'open';
    this.showEditFlagDialog = true;
  }

  closeEditFlagDialog(): void {
    this.showEditFlagDialog = false;
    this.editingFlag = null;
  }

  saveEditFlag(): void {
    if (!this.editingFlag) return;
    this.isSavingFlag = true;

    // Use resolveFlag to update — it supports changing status and notes
    this.messagingApi.resolveFlag(this.editingFlag.id, this.editFlagStatus, `[EDIT] Reason: ${this.editFlagReason}. Details: ${this.editFlagDetails}`).subscribe({
      next: () => {
        this.isSavingFlag = false;
        this.closeEditFlagDialog();
        this.loadFlags();
      },
      error: () => {
        this.isSavingFlag = false;
      }
    });
  }

  get filteredFlags(): MessageFlagSummary[] {
    return this.flags;
  }


  // ─── Audit Log ───────────────────────────────────────────────────────────

  loadAuditLog(): void {
    this.isLoadingAudit = true;

    const action = this.auditActionFilter || undefined;
    const startDate = this.auditDateFilter || undefined;

    this.messagingApi.getAuditLog(startDate, undefined, action, 200).subscribe({
      next: (data) => {
        this.auditLog = data;
        this.isLoadingAudit = false;
        this.auditLoaded = true;
      },
      error: () => {
        this.isLoadingAudit = false;
        this.auditLoaded = true;
      }
    });
  }

  onAuditFilterChange(): void {
    this.loadAuditLog();
  }


  // ─── Delivery Log ───────────────────────────────────────────────────────

  loadDeliveryLogs(): void {
    this.isLoadingDelivery = true;

    const providerId = this.deliveryProviderFilter || undefined;
    const successOnly = this.deliveryStatusFilter === 'success' ? true
                      : this.deliveryStatusFilter === 'failure' ? false
                      : undefined;

    this.messagingApi.getDeliveryLogs(undefined, providerId, successOnly, undefined, 200).subscribe({
      next: (data) => {
        this.deliveryLogs = data;
        this.isLoadingDelivery = false;
        this.deliveryLoaded = true;
      },
      error: () => {
        this.isLoadingDelivery = false;
        this.deliveryLoaded = true;
      }
    });
  }

  onDeliveryFilterChange(): void {
    this.loadDeliveryLogs();
  }


  // ─── Helpers ─────────────────────────────────────────────────────────────

  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return d.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' })
      + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  getFlagStatusClass(flag: MessageFlagSummary): string {
    if (flag.status === 'Resolved' || flag.status === 'dismissed') return 'status-resolved';
    if (flag.status === 'Open' || flag.status === 'pending') return 'status-open';
    return 'status-other';
  }


  // ─── Messages (Admin Search) ─────────────────────────────────────────────

  loadMessages(): void {
    this.isLoadingMessages = true;

    const query = this.messageSearchQuery || undefined;
    const conversationId = this.messageConversationFilter ? parseInt(this.messageConversationFilter, 10) : undefined;
    const startDate = this.messageDateFilter || undefined;

    this.messagingApi.adminSearchMessages(query, conversationId, undefined, startDate, undefined, 200).subscribe({
      next: (data) => {
        this.messageSearchResults = data;
        this.isLoadingMessages = false;
      },
      error: () => {
        this.isLoadingMessages = false;
      }
    });
  }

  onMessageSearchChange(): void {
    // Debounce to avoid excessive API calls during typing
    if (this.messageSearchTimeout) clearTimeout(this.messageSearchTimeout);
    this.messageSearchTimeout = setTimeout(() => this.loadMessages(), 400);
  }

  onMessageFilterChange(): void {
    this.loadMessages();
  }

  flagMessageFromSearch(msg: AdminMessageResult): void {
    this.flaggingMessage = msg;
    this.flagReason = '';
    this.flagDetails = '';
    this.showFlagDialog = true;
  }

  closeFlagDialog(): void {
    this.showFlagDialog = false;
    this.flaggingMessage = null;
  }

  submitFlag(): void {
    if (!this.flaggingMessage || !this.flagReason) return;
    this.isFlagging = true;

    const reason = this.flagReasonOptions.find(r => r.value === this.flagReason)?.label || this.flagReason;

    this.messagingApi.createFlag(this.flaggingMessage.id, reason, this.flagDetails || undefined).subscribe({
      next: () => {
        // Track this message as flagged so we show it in the list
        this.flaggedMessages.set(this.flaggingMessage!.id, {
          reason,
          details: this.flagDetails
        });
        this.isFlagging = false;
        this.closeFlagDialog();
      },
      error: () => {
        this.isFlagging = false;
      }
    });
  }

  isFlagged(messageId: number): boolean {
    return this.flaggedMessages.has(messageId);
  }

  getFlagInfo(messageId: number): { reason: string; details: string } | undefined {
    return this.flaggedMessages.get(messageId);
  }

  toggleFlagDetails(messageId: number): void {
    this.expandedFlagId = this.expandedFlagId === messageId ? null : messageId;
  }

  messageUser(userId: number, displayName: string): void {
    // Navigate to the messaging page — the user can start a DM from there
    this.router.navigate(['/messaging'], { queryParams: { userId, userName: displayName } });
  }


  // ─── Entities ────────────────────────────────────────────────────────────

  loadEntityConversations(): void {
    // Use browseChannels to get all conversations, filter to those with entity links
    this.messagingApi.browseChannels().subscribe({
      next: (data) => {
        this.entityConversations = data.filter(c => c.entity && c.entity.trim() !== '');
      }
    });
  }

  get entityTypes(): string[] {
    const types = new Set<string>();
    for (const c of this.entityConversations) {
      if (c.entity) types.add(c.entity);
    }
    return Array.from(types).sort();
  }

  get filteredEntityConversations(): ConversationSummary[] {
    return this.entityConversations.filter(c => {
      if (this.entityTypeFilter && c.entity !== this.entityTypeFilter) return false;
      if (this.entityIdFilter && c.entityId !== this.entityIdFilter) return false;
      return true;
    });
  }

  searchByEntity(): void {
    if (!this.entityTypeFilter && !this.entityIdFilter) return;
    this.isLoadingEntities = true;
    this.entityResults = [];

    // Find conversations matching the entity filter, then search messages in those conversations
    const matching = this.filteredEntityConversations;
    if (matching.length === 0) {
      this.isLoadingEntities = false;
      return;
    }

    // Fetch messages from the first matching conversation (or all if desired)
    const convId = matching.length === 1 ? matching[0].id : undefined;
    this.messagingApi.adminSearchMessages(undefined, convId, undefined, undefined, undefined, 500).subscribe({
      next: (data) => {
        this.entityResults = data;
        this.isLoadingEntities = false;
      },
      error: () => {
        this.isLoadingEntities = false;
      }
    });
  }


  // ─── Transcript Downloads ───────────────────────────────────────────────

  downloadingTranscripts: { [id: number]: boolean } = {};

  downloadChannelTranscript(ch: ConversationSummary): void {
    this.downloadingTranscripts[ch.id] = true;

    this.messagingApi.adminSearchMessages(undefined, ch.id, undefined, undefined, undefined, 5000).subscribe({
      next: (messages) => {
        // Sort chronologically
        const sorted = [...messages].sort((a, b) =>
          new Date(a.dateTimeCreated).getTime() - new Date(b.dateTimeCreated).getTime()
        );

        // Build transcript
        const lines: string[] = [];
        lines.push(`Transcript: ${ch.name || 'Conversation #' + ch.id}`);
        lines.push(`Exported: ${new Date().toISOString()}`);
        lines.push(`Messages: ${sorted.length}`);
        lines.push('─'.repeat(60));
        lines.push('');

        for (const msg of sorted) {
          const ts = new Date(msg.dateTimeCreated).toLocaleString();
          const deleted = msg.isDeleted ? ' [DELETED]' : '';
          lines.push(`[${ts}] ${msg.userDisplayName}${deleted}`);
          lines.push(msg.message || '(no content)');
          lines.push('');
        }

        this.triggerDownload(
          lines.join('\n'),
          `transcript-${(ch.name || 'conversation').replace(/[^a-zA-Z0-9]/g, '_')}-${new Date().toISOString().slice(0, 10)}.txt`,
          'text/plain'
        );
        this.downloadingTranscripts[ch.id] = false;
      },
      error: () => {
        this.downloadingTranscripts[ch.id] = false;
      }
    });
  }

  downloadSearchResultsCsv(): void {
    if (this.messageSearchResults.length === 0) return;

    const rows: string[] = [];
    rows.push('Date,Conversation,Sender,Message,Flagged,Deleted');

    for (const msg of this.messageSearchResults) {
      const date = new Date(msg.dateTimeCreated).toLocaleString();
      const conv = this.escapeCsv(msg.conversationName);
      const sender = this.escapeCsv(msg.userDisplayName);
      const message = this.escapeCsv(msg.message || '');
      const flagged = this.isFlagged(msg.id) ? 'Yes' : 'No';
      const deleted = msg.isDeleted ? 'Yes' : 'No';
      rows.push(`${date},${conv},${sender},${message},${flagged},${deleted}`);
    }

    this.triggerDownload(
      rows.join('\n'),
      `messages-export-${new Date().toISOString().slice(0, 10)}.csv`,
      'text/csv'
    );
  }

  private escapeCsv(value: string): string {
    if (value.includes(',') || value.includes('"') || value.includes('\n')) {
      return '"' + value.replace(/"/g, '""') + '"';
    }
    return value;
  }

  private triggerDownload(content: string, filename: string, mimeType: string): void {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  truncateMessage(msg: string, maxLen: number = 120): string {
    if (!msg) return '—';
    return msg.length > maxLen ? msg.substring(0, maxLen) + '…' : msg;
  }


  // ─── Analytics ──────────────────────────────────────────────────────────────

  loadAnalytics(): void {
    this.isLoadingAnalytics = true;
    this.messagingApi.getMessagingAnalytics().subscribe({
      next: (data) => {
        this.analytics = data;
        this.isLoadingAnalytics = false;
        this.analyticsLoaded = true;
        // Render charts after Angular has time to render the canvas elements via *ngIf
        setTimeout(() => this.renderCharts(), 300);
      },
      error: () => {
        this.isLoadingAnalytics = false;
        this.analyticsLoaded = true;
      }
    });
  }

  private renderCharts(): void {
    // Destroy previous instances
    this.chartInstances.forEach(c => c.destroy());
    this.chartInstances = [];

    if (!this.analytics) return;

    this.renderMessagesOverTimeChart();
    this.renderTopUsersChart();
    this.renderTopChannelsChart();
    this.buildSankey();
  }

  private renderMessagesOverTimeChart(): void {
    const canvas = this.messagesChartRef?.nativeElement;
    if (!canvas || !this.analytics?.messagesOverTime?.length) return;

    const labels = this.analytics.messagesOverTime.map(d => {
      const date = new Date(d.date);
      return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
    });
    const data = this.analytics.messagesOverTime.map(d => d.count);

    const chart = new Chart(canvas, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: 'Messages',
          data,
          borderColor: '#4d8df7',
          backgroundColor: 'rgba(77, 141, 247, 0.1)',
          fill: true,
          tension: 0.3,
          pointRadius: 3,
          pointBackgroundColor: '#4d8df7',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#22262b',
            titleColor: '#f1f1f2',
            bodyColor: '#a0a3a8',
            borderColor: 'rgba(255,255,255,0.1)',
            borderWidth: 1
          }
        },
        scales: {
          x: {
            ticks: { color: '#71747a', font: { size: 11 }, maxRotation: 45 },
            grid: { color: 'rgba(255,255,255,0.04)' }
          },
          y: {
            beginAtZero: true,
            ticks: { color: '#71747a', font: { size: 11 } },
            grid: { color: 'rgba(255,255,255,0.06)' }
          }
        }
      }
    });
    this.chartInstances.push(chart);
  }

  private renderTopUsersChart(): void {
    const canvas = this.topUsersChartRef?.nativeElement;
    if (!canvas || !this.analytics?.topUsers?.length) return;

    const labels = this.analytics.topUsers.map(u => u.displayName);
    const data = this.analytics.topUsers.map(u => u.messageCount);

    const colors = [
      '#4d8df7', '#7b61ff', '#2ecc71', '#f59e0b', '#06b6d4',
      '#f472b6', '#8b5cf6', '#34d399', '#fb923c', '#60a5fa',
      '#a78bfa', '#22d3ee', '#fbbf24', '#f87171', '#c084fc'
    ];

    const chart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Messages',
          data,
          backgroundColor: colors.slice(0, data.length).map(c => c + '40'),
          borderColor: colors.slice(0, data.length),
          borderWidth: 1
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#22262b',
            titleColor: '#f1f1f2',
            bodyColor: '#a0a3a8',
            borderColor: 'rgba(255,255,255,0.1)',
            borderWidth: 1
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            ticks: { color: '#71747a', font: { size: 11 } },
            grid: { color: 'rgba(255,255,255,0.06)' }
          },
          y: {
            ticks: { color: '#a0a3a8', font: { size: 12 } },
            grid: { display: false }
          }
        }
      }
    });
    this.chartInstances.push(chart);
  }

  private renderTopChannelsChart(): void {
    const canvas = this.topChannelsChartRef?.nativeElement;
    if (!canvas || !this.analytics?.topChannels?.length) return;

    const labels = this.analytics.topChannels.map(c => c.name);
    const data = this.analytics.topChannels.map(c => c.messageCount);

    const colors = [
      '#06b6d4', '#8b5cf6', '#2ecc71', '#f59e0b', '#4d8df7',
      '#f472b6', '#34d399', '#fb923c', '#60a5fa', '#e63946'
    ];

    const chart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Messages',
          data,
          backgroundColor: colors.slice(0, data.length).map(c => c + '40'),
          borderColor: colors.slice(0, data.length),
          borderWidth: 1
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#22262b',
            titleColor: '#f1f1f2',
            bodyColor: '#a0a3a8',
            borderColor: 'rgba(255,255,255,0.1)',
            borderWidth: 1
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            ticks: { color: '#71747a', font: { size: 11 } },
            grid: { color: 'rgba(255,255,255,0.06)' }
          },
          y: {
            ticks: { color: '#a0a3a8', font: { size: 12 } },
            grid: { display: false }
          }
        }
      }
    });
    this.chartInstances.push(chart);
  }

  private buildSankey(): void {
    if (!this.analytics?.userFlows?.length) {
      this.sankeyPaths = [];
      this.sankeyLabels = [];
      return;
    }

    const flows = this.analytics.userFlows;
    const svgW = 700;
    const svgH = 400;
    const nodeW = 18;
    const padX = 140;

    // Collect unique senders and receivers
    const senders = [...new Set(flows.map(f => f.fromUser))];
    const receivers = [...new Set(flows.map(f => f.toUser))];

    const leftX = padX;
    const rightX = svgW - padX;

    // Calculate node heights proportional to flow weights
    const senderWeights = new Map<string, number>();
    const receiverWeights = new Map<string, number>();
    for (const f of flows) {
      senderWeights.set(f.fromUser, (senderWeights.get(f.fromUser) || 0) + f.weight);
      receiverWeights.set(f.toUser, (receiverWeights.get(f.toUser) || 0) + f.weight);
    }

    const maxWeight = Math.max(...[...senderWeights.values(), ...receiverWeights.values()]);
    const usableH = svgH - 40;
    const senderGap = Math.max(4, (usableH - senders.length * 20) / (senders.length + 1));
    const receiverGap = Math.max(4, (usableH - receivers.length * 20) / (receivers.length + 1));

    // Position sender nodes
    const senderPositions = new Map<string, { y: number; h: number; usedY: number }>();
    let sY = 20;
    for (const s of senders) {
      const w = senderWeights.get(s) || 1;
      const h = Math.max(8, (w / maxWeight) * (usableH * 0.4));
      senderPositions.set(s, { y: sY, h, usedY: sY });
      sY += h + senderGap;
    }

    // Position receiver nodes
    const receiverPositions = new Map<string, { y: number; h: number; usedY: number }>();
    let rY = 20;
    for (const r of receivers) {
      const w = receiverWeights.get(r) || 1;
      const h = Math.max(8, (w / maxWeight) * (usableH * 0.4));
      receiverPositions.set(r, { y: rY, h, usedY: rY });
      rY += h + receiverGap;
    }

    const colors = ['#4d8df7', '#7b61ff', '#2ecc71', '#f59e0b', '#06b6d4', '#f472b6', '#8b5cf6', '#34d399', '#fb923c', '#60a5fa'];

    this.sankeyPaths = flows.map((f, i) => {
      const sp = senderPositions.get(f.fromUser)!;
      const rp = receiverPositions.get(f.toUser)!;
      const flowH = Math.max(2, (f.weight / maxWeight) * 30);
      const sy = sp.usedY;
      sp.usedY += flowH + 1;
      const ry = rp.usedY;
      rp.usedY += flowH + 1;

      const x1 = leftX + nodeW;
      const x2 = rightX;
      const cx = (x1 + x2) / 2;

      const d = `M ${x1} ${sy} C ${cx} ${sy}, ${cx} ${ry}, ${x2} ${ry} L ${x2} ${ry + flowH} C ${cx} ${ry + flowH}, ${cx} ${sy + flowH}, ${x1} ${sy + flowH} Z`;

      return {
        d,
        color: colors[i % colors.length],
        opacity: 0.4,
        fromLabel: f.fromUser,
        toLabel: f.toUser,
        weight: f.weight,
        fromY: sy + flowH / 2,
        toY: ry + flowH / 2
      };
    });

    // Labels
    this.sankeyLabels = [];
    for (const s of senders) {
      const pos = senderPositions.get(s)!;
      this.sankeyLabels.push({ text: s, x: leftX - 8, y: pos.y + pos.h / 2, align: 'end' });
    }
    for (const r of receivers) {
      const pos = receiverPositions.get(r)!;
      this.sankeyLabels.push({ text: r, x: rightX + nodeW + 8, y: pos.y + pos.h / 2, align: 'start' });
    }
  }

  get filteredUsers(): UserActivity[] {
    if (!this.analytics?.topUsers) return [];
    if (!this.userSearchFilter.trim()) return this.analytics.topUsers;
    const q = this.userSearchFilter.toLowerCase();
    return this.analytics.topUsers.filter(u => u.displayName.toLowerCase().includes(q));
  }
}
