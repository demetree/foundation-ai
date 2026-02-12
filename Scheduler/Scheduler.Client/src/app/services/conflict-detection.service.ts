/**
 * ConflictDetectionService
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Client-side scheduling conflict detection.
 *
 * Detects time overlaps between events sharing the same resourceId or crewId.
 * Operates on the already-loaded calendar events (including server-expanded
 * recurrence instances), so no additional server calls are needed.
 */
import { Injectable } from '@angular/core';
import { ScheduledEventData } from '../scheduler-data-services/scheduled-event.service';

// ────────────────────────────────────────────────────────────────────────────
// Public interfaces
// ────────────────────────────────────────────────────────────────────────────

export type ConflictType = 'resource' | 'crew';

export interface ScheduleConflict {
    /** First event in the conflicting pair */
    eventA: ScheduledEventData;
    /** Second event in the conflicting pair */
    eventB: ScheduledEventData;
    /** Whether conflict is on a shared resource or crew */
    type: ConflictType;
    /** The shared entity's ID */
    entityId: number | bigint;
    /** Human-readable entity name (set by caller if available) */
    entityName: string;
    /** Number of minutes the two events overlap */
    overlapMinutes: number;
}

// ────────────────────────────────────────────────────────────────────────────
// Service
// ────────────────────────────────────────────────────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class ConflictDetectionService {

    /**
     * Detects scheduling conflicts among a list of events.
     *
     * Two events conflict if:
     *   1. They share the same non-null resourceId  OR  crewId
     *   2. Their time ranges overlap (startA < endB && startB < endA)
     *   3. They are different events (different id, or different recurrenceInstanceDate)
     *
     * @param events - Array of ScheduledEventData (can include virtual recurrence instances)
     * @returns Array of detected conflicts
     */
    detectConflicts(events: ScheduledEventData[]): ScheduleConflict[] {
        const conflicts: ScheduleConflict[] = [];
        const seen = new Set<string>(); // Dedup key: "typeA-typeB-entityId"

        // --- Resource conflicts ---
        const byResource = this.groupBy(events, e => e.resourceId);
        for (const [resourceId, group] of byResource) {
            this.findOverlaps(group, 'resource', resourceId, conflicts, seen);
        }

        // --- Crew conflicts ---
        const byCrew = this.groupBy(events, e => e.crewId);
        for (const [crewId, group] of byCrew) {
            this.findOverlaps(group, 'crew', crewId, conflicts, seen);
        }

        return conflicts;
    }

    /**
     * Returns the set of event IDs that appear in at least one conflict.
     */
    getConflictEventIds(conflicts: ScheduleConflict[]): Set<number> {
        const ids = new Set<number>();
        for (const c of conflicts) {
            ids.add(c.eventA.id as number);
            ids.add(c.eventB.id as number);
        }
        return ids;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────────────────────────

    /**
     * Groups events by a numeric foreign-key accessor, skipping nulls/zeros.
     */
    private groupBy(
        events: ScheduledEventData[],
        accessor: (e: ScheduledEventData) => bigint | number | null | undefined
    ): Map<number, ScheduledEventData[]> {
        const map = new Map<number, ScheduledEventData[]>();

        for (const event of events) {
            const raw = accessor(event);
            if (raw == null || raw === 0) continue;
            const key = Number(raw);
            if (!map.has(key)) map.set(key, []);
            map.get(key)!.push(event);
        }

        return map;
    }

    /**
     * Within a group sharing the same entity, find all pairwise time overlaps.
     */
    private findOverlaps(
        group: ScheduledEventData[],
        type: ConflictType,
        entityId: number | bigint,
        conflicts: ScheduleConflict[],
        seen: Set<string>
    ): void {
        for (let i = 0; i < group.length; i++) {
            for (let j = i + 1; j < group.length; j++) {
                const a = group[i];
                const b = group[j];

                // Skip if same event (by id and instance date)
                if (this.isSameEvent(a, b)) continue;

                const overlap = this.calculateOverlapMinutes(a, b);
                if (overlap > 0) {
                    // Dedup key: ordered pair of IDs + type
                    const idA = Math.min(Number(a.id), Number(b.id));
                    const idB = Math.max(Number(a.id), Number(b.id));
                    const key = `${type}-${idA}-${idB}-${Number(entityId)}`;
                    if (seen.has(key)) continue;
                    seen.add(key);

                    conflicts.push({
                        eventA: a,
                        eventB: b,
                        type,
                        entityId,
                        entityName: '', // Populated by caller
                        overlapMinutes: overlap
                    });
                }
            }
        }
    }

    /** Two references point to the same logical event */
    private isSameEvent(a: ScheduledEventData, b: ScheduledEventData): boolean {
        if (Number(a.id) !== Number(b.id)) return false;
        // Different recurrence instances of the same master have different instance dates
        if (a.recurrenceInstanceDate !== b.recurrenceInstanceDate) return false;
        return true;
    }

    /** Calculate overlap in minutes between two events (0 if no overlap) */
    private calculateOverlapMinutes(a: ScheduledEventData, b: ScheduledEventData): number {
        const startA = new Date(a.startDateTime).getTime();
        const endA = new Date(a.endDateTime).getTime();
        const startB = new Date(b.startDateTime).getTime();
        const endB = new Date(b.endDateTime).getTime();

        // No overlap
        if (startA >= endB || startB >= endA) return 0;

        const overlapStart = Math.max(startA, startB);
        const overlapEnd = Math.min(endA, endB);

        return Math.round((overlapEnd - overlapStart) / 60000);
    }
}
