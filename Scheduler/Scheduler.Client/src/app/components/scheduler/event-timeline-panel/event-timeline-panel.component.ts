/**
 * EventTimelinePanelComponent
 *
 * AI-Developed — Aggregated event lifecycle timeline showing all changes
 * to an event and its related entities (charges, documents, calendar links,
 * assignments, recurrence rules, financial transactions).
 *
 * Features:
 *   - Horizontal scatter timeline chart (ng2-charts)
 *   - Expandable activity table with field-level diffs
 *   - Color-coded entity type badges
 *   - Summary stats (total changes, date range, contributors)
 */

import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { lastValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ChartConfiguration, ChartData, TooltipItem } from 'chart.js';
import 'chartjs-adapter-date-fns';
import { BaseChartDirective } from 'ng2-charts';
import { ScheduledEventData, ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { RecurrenceRuleService } from '../../../scheduler-data-services/recurrence-rule.service';


// ─── Interfaces ──────────────────────────────────────────────────────────────

interface FieldDiff {
  field: string;
  label: string;
  oldValue: any;
  newValue: any;
}

interface TimelineEntry {
  timestamp: Date;
  entityType: string;
  entityTypeIcon: string;
  entityTypeColor: string;
  action: 'Created' | 'Updated';
  userName: string;
  userId: bigint | number;
  versionNumber: number;
  diffs: FieldDiff[];
  rawData: any;
  previousData: any;
  entityLabel: string;   // e.g. "Charge: Rental Fee"
}


// ─── Entity type configuration ───────────────────────────────────────────────

const ENTITY_TYPES: Record<string, { icon: string; color: string; label: string }> = {
  'Event':       { icon: 'bi-calendar-event',     color: '#6366f1', label: 'Event' },
  'Charge':      { icon: 'bi-currency-dollar',    color: '#10b981', label: 'Charge' },
  'Calendar':    { icon: 'bi-calendar2-week',     color: '#f59e0b', label: 'Calendar Link' },
  'Document':    { icon: 'bi-file-earmark-text',  color: '#3b82f6', label: 'Document' },
  'Assignment':  { icon: 'bi-person-badge',       color: '#ec4899', label: 'Assignment' },
  'Recurrence':  { icon: 'bi-arrow-repeat',       color: '#8b5cf6', label: 'Recurrence' },
};


@Component({
  selector: 'app-event-timeline-panel',
  templateUrl: './event-timeline-panel.component.html',
  styleUrls: ['./event-timeline-panel.component.scss']
})
export class EventTimelinePanelComponent implements OnInit {

  @Input() event!: ScheduledEventData;

  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

  // State
  loading = true;
  timelineEntries: TimelineEntry[] = [];
  expandedIndex: number | null = null;

  // Summary stats
  totalChanges = 0;
  uniqueContributors = 0;
  dateRange = '';

  // Entity type config (for template access)
  entityTypes = ENTITY_TYPES;

  // Fields to exclude from diffs (internal/noise)
  private readonly excludeFields = new Set([
    'id', 'objectGuid', 'versionNumber', 'active', 'deleted',
    'scheduledEventId', 'eventChargeId', 'eventCalendarId',
    'documentId', 'eventResourceAssignmentId', 'recurrenceRuleId',
    'fileDataData', 'fileDataFileName', 'fileDataMimeType', 'fileDataSize',
    'avatarData',
  ]);

  // ─── Chart ─────────────────────────────────────────────────────────────────

  chartType: 'scatter' = 'scatter';
  chartData: ChartData<'scatter'> = { datasets: [] };
  chartOptions: ChartConfiguration<'scatter'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        type: 'time' as any,
        time: {
          tooltipFormat: 'MMM d, yyyy h:mm a',
          displayFormats: {
            hour: 'h:mm a',
            day: 'MMM d',
            month: 'MMM yyyy'
          }
        },
        title: { display: false },
        grid: { display: false },
        ticks: {
          maxTicksLimit: 8,
          maxRotation: 45,
          font: { size: 10 }
        }
      },
      y: {
        display: false,
        min: -0.5,
        max: 0.5
      }
    },
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
        labels: {
          usePointStyle: true,
          pointStyle: 'circle',
          padding: 16,
          font: { size: 11 }
        }
      },
      tooltip: {
        callbacks: {
          label: (ctx: TooltipItem<'scatter'>) => {
            const entry = (ctx.raw as any)._entry as TimelineEntry;
            if (!entry) return '';
            const action = entry.action === 'Created' ? '✦ Created' : `✎ Updated (${entry.diffs.length} field${entry.diffs.length !== 1 ? 's' : ''})`;
            return `${entry.entityType}: ${action} by ${entry.userName}`;
          }
        }
      }
    }
  };


  constructor(
    private scheduledEventService: ScheduledEventService,
    private eventChargeService: EventChargeService,
    private documentService: DocumentService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private recurrenceRuleService: RecurrenceRuleService
  ) {}


  ngOnInit(): void {
    this.loadTimeline();
  }


  // ─── Data Loading ──────────────────────────────────────────────────────────

  async loadTimeline(): Promise<void> {
    this.loading = true;
    const entries: TimelineEntry[] = [];
    const eventId = this.event.id as number;

    try {
      // 1. ScheduledEvent audit history (includes resolved user info)
      const eventHistory = await lastValueFrom(
        this.scheduledEventService.GetScheduledEventAuditHistory(eventId, true)
          .pipe(catchError(() => of([])))
      );
      this.processAuditHistoryGroup(eventHistory, 'Event', entries);

      // 2. EventCharge — get charges for this event, then their audit histories
      const charges = await lastValueFrom(
        this.eventChargeService.GetEventChargeList({ scheduledEventId: eventId })
          .pipe(catchError(() => of([])))
      );
      for (const charge of charges) {
        const chargeHistory = await lastValueFrom(
          this.eventChargeService.GetEventChargeAuditHistory(charge.id as number, true)
            .pipe(catchError(() => of([])))
        );
        this.processAuditHistoryGroup(chargeHistory, 'Charge', entries);
      }

      // 3. EventCalendar — no change history table exists
      // (Skipped — EventCalendarChangeHistory service not available)

      // 4. Documents
      const docs = await lastValueFrom(
        this.documentService.GetDocumentList({ scheduledEventId: eventId })
          .pipe(catchError(() => of([])))
      );
      for (const doc of docs) {
        const docHistory = await lastValueFrom(
          this.documentService.GetDocumentAuditHistory(doc.id as number, true)
            .pipe(catchError(() => of([])))
        );
        this.processAuditHistoryGroup(docHistory, 'Document', entries);
      }

      // 5. EventResourceAssignment
      const assignments = await lastValueFrom(
        this.eventResourceAssignmentService.GetEventResourceAssignmentList({ scheduledEventId: eventId })
          .pipe(catchError(() => of([])))
      );
      for (const assignment of assignments) {
        const assignHistory = await lastValueFrom(
          this.eventResourceAssignmentService.GetEventResourceAssignmentAuditHistory(assignment.id as number, true)
            .pipe(catchError(() => of([])))
        );
        this.processAuditHistoryGroup(assignHistory, 'Assignment', entries);
      }

      // 6. RecurrenceRule
      const rules = await lastValueFrom(
        this.recurrenceRuleService.GetRecurrenceRuleList({ scheduledEventId: eventId })
          .pipe(catchError(() => of([])))
      );
      for (const rule of rules) {
        const ruleHistory = await lastValueFrom(
          this.recurrenceRuleService.GetRecurrenceRuleAuditHistory(rule.id as number, true)
            .pipe(catchError(() => of([])))
        );
        this.processAuditHistoryGroup(ruleHistory, 'Recurrence', entries);
      }

    } catch (err) {
      console.error('Failed to load event timeline', err);
    }

    // Sort chronologically (newest first for the table)
    entries.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());
    this.timelineEntries = entries;

    // Summary stats
    this.totalChanges = entries.length;
    const users = new Set(entries.map(e => e.userId.toString()));
    this.uniqueContributors = users.size;

    if (entries.length > 0) {
      const oldest = entries[entries.length - 1].timestamp;
      const newest = entries[0].timestamp;
      this.dateRange = `${oldest.toLocaleDateString()} — ${newest.toLocaleDateString()}`;
    }

    // Build chart
    this.buildChart();

    this.loading = false;
  }


  /**
   * Processes a group of VersionInformation<T> audit history records
   * (all for the same entity type) and adds them to the entries array.
   * User names are resolved server-side via the VersionInformation.user property.
   */
  private processAuditHistoryGroup(
    records: any[],
    entityType: string,
    entries: TimelineEntry[]
  ): void {

    if (!records || records.length === 0) return;

    // Sort by version number ascending so we can compute diffs sequentially
    const sorted = [...records].sort((a, b) => (a.versionNumber || 0) - (b.versionNumber || 0));

    for (let i = 0; i < sorted.length; i++) {
      const record = sorted[i];
      const isFirst = i === 0;
      const prev = isFirst ? null : sorted[i - 1];

      let currentData: any = null;
      let previousData: any = null;

      try { currentData = typeof record.data === 'string' ? JSON.parse(record.data) : record.data; } catch {}
      try { previousData = prev && typeof prev.data === 'string' ? JSON.parse(prev.data) : prev?.data || null; } catch {}

      const diffs = isFirst ? [] : this.computeDiffs(previousData, currentData);
      const config = ENTITY_TYPES[entityType] || ENTITY_TYPES['Event'];
      const entityLabel = this.buildEntityLabel(entityType, currentData);

      // Resolve user name from VersionInformation.user (server-resolved)
      const user = record.user;
      let userName = 'Unknown';
      if (user) {
        const parts = [user.firstName, user.lastName].filter(Boolean);
        userName = parts.length > 0 ? parts.join(' ') : (user.userName || 'Unknown');
      }

      entries.push({
        timestamp: new Date(record.timeStamp),
        entityType,
        entityTypeIcon: config.icon,
        entityTypeColor: config.color,
        action: isFirst ? 'Created' : 'Updated',
        userName,
        userId: user?.id ?? 0,
        versionNumber: record.versionNumber || 0,
        diffs,
        rawData: currentData,
        previousData,
        entityLabel
      });
    }
  }


  /**
   * Builds a human-readable label for the entity, e.g. "Charge: Rental Fee"
   */
  private buildEntityLabel(entityType: string, data: any): string {
    if (!data) return entityType;

    switch (entityType) {
      case 'Event': return data.name || 'Event';
      case 'Charge': return data.description || 'Charge';
      case 'Calendar': return data.calendarName || 'Calendar Link';
      case 'Document': return data.name || data.fileName || 'Document';
      case 'Assignment': return data.resourceName || 'Assignment';
      case 'Recurrence': return data.frequency || 'Recurrence Rule';
      default: return entityType;
    }
  }


  // ─── Diff Computation ──────────────────────────────────────────────────────

  private computeDiffs(oldData: any, newData: any): FieldDiff[] {
    if (!oldData || !newData) return [];

    const diffs: FieldDiff[] = [];
    const allKeys = new Set<string>([
      ...Object.keys(oldData),
      ...Object.keys(newData)
    ]);

    for (const key of allKeys) {
      if (this.excludeFields.has(key)) continue;
      if (key.startsWith('_')) continue;
      if (typeof oldData[key] === 'object' && oldData[key] !== null && !Array.isArray(oldData[key])) continue;
      if (typeof newData[key] === 'object' && newData[key] !== null && !Array.isArray(newData[key])) continue;

      const oldVal = oldData[key] ?? null;
      const newVal = newData[key] ?? null;

      if (oldVal !== newVal && JSON.stringify(oldVal) !== JSON.stringify(newVal)) {
        diffs.push({
          field: key,
          label: this.humanizeFieldName(key),
          oldValue: oldVal,
          newValue: newVal
        });
      }
    }

    return diffs.sort((a, b) => a.label.localeCompare(b.label));
  }


  // ─── Chart Building ────────────────────────────────────────────────────────

  private buildChart(): void {
    // Group entries by entity type
    const groups = new Map<string, TimelineEntry[]>();
    for (const entry of this.timelineEntries) {
      const list = groups.get(entry.entityType) || [];
      list.push(entry);
      groups.set(entry.entityType, list);
    }

    const datasets: any[] = [];
    for (const [type, entries] of groups) {
      const config = ENTITY_TYPES[type] || ENTITY_TYPES['Event'];
      datasets.push({
        label: config.label,
        data: entries.map(e => ({
          x: e.timestamp.getTime(),
          y: (Math.random() - 0.5) * 0.3,  // Slight vertical jitter to avoid overlap
          _entry: e
        })),
        backgroundColor: config.color,
        borderColor: config.color,
        pointRadius: entries.map(e => e.action === 'Created' ? 8 : Math.min(4 + e.diffs.length, 10)),
        pointHoverRadius: 12,
        pointStyle: entries.map(e => e.action === 'Created' ? 'rectRounded' : 'circle'),
      });
    }

    this.chartData = { datasets };
  }


  // ─── Display Helpers ───────────────────────────────────────────────────────

  humanizeFieldName(field: string): string {
    return field
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, s => s.toUpperCase())
      .replace(/ Id$/, '')
      .trim();
  }

  formatValue(val: any): string {
    if (val === null || val === undefined) return '—';
    if (typeof val === 'boolean') return val ? 'Yes' : 'No';
    if (typeof val === 'string' && val.length === 0) return '(empty)';
    if (typeof val === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(val)) {
      try { return new Date(val).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' }); } catch { return val; }
    }
    if (typeof val === 'number') {
      // Format currency-like numbers
      if (Number.isInteger(val) || val.toString().includes('.')) {
        return val.toLocaleString();
      }
    }
    return String(val);
  }

  getRelativeTime(date: Date): string {
    const now = Date.now();
    const diffMs = now - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)}w ago`;
    return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
  }

  toggleExpand(index: number): void {
    this.expandedIndex = this.expandedIndex === index ? null : index;
  }

  trackByIndex(index: number): number {
    return index;
  }
}
