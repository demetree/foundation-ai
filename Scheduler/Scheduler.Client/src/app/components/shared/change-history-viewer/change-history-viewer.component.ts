/**
 * ChangeHistoryViewerComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Universal, reusable component for displaying version history (audit trail)
 * for any entity that uses Foundation version control.
 *
 * Accepts pre-fetched VersionInformation<T>[] data and renders a polished
 * timeline with expandable per-version field diffs.
 *
 * Usage:
 *   <app-change-history-viewer
 *       [auditHistory]="auditHistory"
 *       [isLoading]="isLoadingHistory"
 *       [entityName]="'Contact'"
 *       [excludeFields]="['objectGuid', 'avatarData']">
 *   </app-change-history-viewer>
 */
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

export interface FieldDiff {
    field: string;
    label: string;
    oldValue: any;
    newValue: any;
    isLongText: boolean;
}

export interface ProcessedHistoryEntry {
    versionNumber: number;
    timeStamp: string;
    userName: string;
    diffs: FieldDiff[];
    isExpanded: boolean;
    isInitialCreation: boolean;
}

@Component({
    selector: 'app-change-history-viewer',
    templateUrl: './change-history-viewer.component.html',
    styleUrls: ['./change-history-viewer.component.scss']
})
export class ChangeHistoryViewerComponent implements OnChanges {

    @Input() auditHistory: any[] | null = null;
    @Input() isLoading: boolean = false;
    @Input() entityName: string = '';
    @Input() excludeFields: string[] = [];
    @Input() fieldLabels: { [key: string]: string } = {};

    public processedEntries: ProcessedHistoryEntry[] = [];

    // Fields always excluded from diffs (internal/noise)
    private readonly alwaysExclude = [
        'id', 'objectGuid', 'versionNumber', 'active', 'deleted',
        // Navigation properties & lazy-loading internals (start with _)
    ];

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['auditHistory'] && this.auditHistory) {
            this.processHistory();
        }
    }

    // =========================================================================
    // Processing
    // =========================================================================

    private processHistory(): void {
        if (!this.auditHistory || this.auditHistory.length === 0) {
            this.processedEntries = [];
            return;
        }

        // Sort descending by version number (newest first)
        const sorted = [...this.auditHistory].sort((a, b) =>
            (b.versionNumber || 0) - (a.versionNumber || 0)
        );

        this.processedEntries = sorted.map((entry, index) => {
            const olderEntry = index < sorted.length - 1 ? sorted[index + 1] : null;
            const isInitial = !olderEntry;

            return {
                versionNumber: entry.versionNumber || 0,
                timeStamp: entry.timeStamp || '',
                userName: this.resolveUserName(entry),
                diffs: isInitial ? [] : this.computeDiffs(olderEntry?.data, entry.data),
                isExpanded: false,
                isInitialCreation: isInitial
            };
        });
    }

    private resolveUserName(entry: any): string {
        if (!entry?.user) return 'System';

        const u = entry.user;
        const parts: string[] = [];
        if (u.firstName) parts.push(u.firstName);
        if (u.lastName) parts.push(u.lastName);
        if (parts.length > 0) return parts.join(' ');
        if (u.userName) return u.userName;
        return 'User #' + (u.id || '?');
    }

    private computeDiffs(oldData: any, newData: any): FieldDiff[] {
        if (!oldData || !newData) return [];

        const diffs: FieldDiff[] = [];
        const allKeys = new Set<string>([
            ...Object.keys(oldData),
            ...Object.keys(newData)
        ]);

        const excluded = new Set([...this.alwaysExclude, ...this.excludeFields]);

        for (const key of allKeys) {
            // Skip excluded, private, and navigation property fields
            if (excluded.has(key)) continue;
            if (key.startsWith('_')) continue;
            if (typeof oldData[key] === 'function' || typeof newData[key] === 'function') continue;
            // Skip complex objects (nav properties)
            if (this.isNavProperty(oldData[key]) || this.isNavProperty(newData[key])) continue;

            const oldVal = oldData[key] ?? null;
            const newVal = newData[key] ?? null;

            if (!this.valuesEqual(oldVal, newVal)) {
                const label = this.fieldLabels[key] || this.humanizeFieldName(key);
                const isLong = this.isLongValue(oldVal) || this.isLongValue(newVal);

                diffs.push({ field: key, label, oldValue: oldVal, newValue: newVal, isLongText: isLong });
            }
        }

        return diffs.sort((a, b) => a.label.localeCompare(b.label));
    }

    private isNavProperty(value: any): boolean {
        return value !== null && typeof value === 'object' && !Array.isArray(value) && !(value instanceof Date);
    }

    private valuesEqual(a: any, b: any): boolean {
        if (a === b) return true;
        if (a == null && b == null) return true;
        // Compare stringified for arrays
        if (Array.isArray(a) && Array.isArray(b)) {
            return JSON.stringify(a) === JSON.stringify(b);
        }
        return false;
    }

    private isLongValue(val: any): boolean {
        return typeof val === 'string' && val.length > 100;
    }

    // =========================================================================
    // Display Helpers
    // =========================================================================

    public humanizeFieldName(field: string): string {
        // Convert camelCase to Title Case with spaces
        // e.g. 'contactTypeId' → 'Contact Type Id'
        return field
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, s => s.toUpperCase())
            .replace(/ Id$/, '')  // Remove trailing 'Id' for FK fields
            .trim();
    }

    public formatValue(val: any): string {
        if (val === null || val === undefined) return '—';
        if (typeof val === 'boolean') return val ? 'Yes' : 'No';
        if (typeof val === 'string' && val.length === 0) return '(empty)';
        // Try to detect ISO date strings
        if (typeof val === 'string' && /^\d{4}-\d{2}-\d{2}T/.test(val)) {
            try {
                const d = new Date(val);
                return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
            } catch { return val; }
        }
        return String(val);
    }

    public getRelativeTime(timestamp: string): string {
        if (!timestamp) return '';
        const now = Date.now();
        const then = new Date(timestamp).getTime();
        const diffMs = now - then;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;
        if (diffDays < 30) return `${Math.floor(diffDays / 7)}w ago`;
        return new Date(timestamp).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
    }

    public toggleExpand(entry: ProcessedHistoryEntry): void {
        entry.isExpanded = !entry.isExpanded;
    }

    public trackByVersion(index: number, entry: ProcessedHistoryEntry): number {
        return entry.versionNumber;
    }
}
