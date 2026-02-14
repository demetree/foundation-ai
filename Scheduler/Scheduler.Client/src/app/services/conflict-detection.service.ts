/**
 * ConflictDetectionService
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Client-side scheduling conflict detection.
 *
 * Detects time overlaps between events sharing the same resourceId or crewId,
 * and optionally detects events scheduled during resource blackout periods.
 * Operates on the already-loaded calendar events (including server-expanded
 * recurrence instances), so no additional server calls are needed.
 */
import { Injectable } from '@angular/core';
import { ScheduledEventData } from '../scheduler-data-services/scheduled-event.service';

// ────────────────────────────────────────────────────────────────────────────
// Public interfaces
// ────────────────────────────────────────────────────────────────────────────

export type ConflictType = 'resource' | 'crew' | 'availability' | 'shift';

export interface ScheduleConflict {
    /** First event in the conflicting pair (for availability: the event in conflict) */
    eventA: ScheduledEventData;
    /** Second event in the conflicting pair (for availability: a synthetic event representing the blackout) */
    eventB: ScheduledEventData;
    /** Whether conflict is on a shared resource, crew, or availability blackout */
    type: ConflictType;
    /** The shared entity's ID (resourceId for availability) */
    entityId: number | bigint;
    /** Human-readable entity name (set by caller if available) */
    entityName: string;
    /** Number of minutes the two events overlap */
    overlapMinutes: number;
    /** Reason for blackout (only set for availability conflicts) */
    blackoutReason?: string;
    /** Human-readable shift window description (only set for shift conflicts) */
    shiftDescription?: string;
}

/** Represents a resource blackout period for availability conflict detection */
export interface BlackoutPeriod {
    resourceId: number;
    resourceName: string;
    startDateTime: string;
    endDateTime: string;
    reason: string;
}

/** Represents an event that violates a resource's shift boundaries */
export interface ShiftViolation {
    /** The event that falls outside the shift window */
    event: ScheduledEventData;
    /** The resource whose shift is violated */
    resourceId: number;
    /** Human-readable resource name */
    resourceName: string;
    /** Day of week the violation is on (0=Sun..6=Sat) */
    dayOfWeek: number;
    /** Formatted shift window descriptions, e.g. ["08:00–16:00 (Day Shift)"] */
    shiftWindows: string[];
    /** Description of the violation for display */
    description: string;
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
     * Optionally checks for availability conflicts (events during blackout periods).
     *
     * @param events - Array of ScheduledEventData (can include virtual recurrence instances)
     * @param blackouts - Optional array of resource blackout periods
     * @returns Array of detected conflicts
     */
    detectConflicts(events: ScheduledEventData[], blackouts?: BlackoutPeriod[]): ScheduleConflict[] {
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

        // --- Availability conflicts (events during blackout periods) ---
        if (blackouts && blackouts.length > 0) {
            const availConflicts = this.detectAvailabilityConflicts(events, blackouts);
            conflicts.push(...availConflicts);
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

    /**
     * Detects events that overlap with resource blackout (unavailability) periods.
     *
     * For each event with a resourceId, checks all blackout periods for that resource.
     * Creates a synthetic ScheduledEventData for each blackout to populate the
     * conflict pair (eventB) for display in the conflict panel.
     */
    detectAvailabilityConflicts(
        events: ScheduledEventData[],
        blackouts: BlackoutPeriod[]
    ): ScheduleConflict[] {
        const conflicts: ScheduleConflict[] = [];
        const seen = new Set<string>();

        // Group blackouts by resourceId for efficient lookup
        const blackoutsByResource = new Map<number, BlackoutPeriod[]>();
        for (const b of blackouts) {
            if (!blackoutsByResource.has(b.resourceId)) {
                blackoutsByResource.set(b.resourceId, []);
            }
            blackoutsByResource.get(b.resourceId)!.push(b);
        }

        for (const event of events) {
            const resId = Number(event.resourceId);
            if (!resId) continue;

            const resourceBlackouts = blackoutsByResource.get(resId);
            if (!resourceBlackouts) continue;

            const eventStart = new Date(event.startDateTime).getTime();
            const eventEnd = new Date(event.endDateTime).getTime();

            for (const blackout of resourceBlackouts) {
                const bStart = new Date(blackout.startDateTime).getTime();
                const bEnd = blackout.endDateTime ? new Date(blackout.endDateTime).getTime() : Infinity;

                // Check overlap
                if (eventStart >= bEnd || eventEnd <= bStart) continue;

                // Dedup key
                const key = `availability-${Number(event.id)}-${resId}-${bStart}`;
                if (seen.has(key)) continue;
                seen.add(key);

                const overlapStart = Math.max(eventStart, bStart);
                const overlapEnd = Math.min(eventEnd, bEnd === Infinity ? eventEnd : bEnd);
                const overlapMinutes = Math.round((overlapEnd - overlapStart) / 60000);

                // Create a synthetic event to represent the blackout in the conflict pair
                const blackoutEvent = new ScheduledEventData();
                blackoutEvent.id = -1 as any;
                blackoutEvent.name = `Blackout: ${blackout.reason || 'Unavailable'}`;
                blackoutEvent.startDateTime = blackout.startDateTime;
                blackoutEvent.endDateTime = blackout.endDateTime || event.endDateTime;

                conflicts.push({
                    eventA: event,
                    eventB: blackoutEvent,
                    type: 'availability',
                    entityId: resId,
                    entityName: blackout.resourceName || `Resource #${resId}`,
                    overlapMinutes,
                    blackoutReason: blackout.reason || 'Unavailable'
                });
            }
        }

        return conflicts;
    }

    /**
     * Creates conflict entries for events that fall outside their assigned resource's shift hours.
     * Called by the calendar component after shift boundary analysis.
     */
    detectShiftConflicts(violations: ShiftViolation[]): ScheduleConflict[] {
        const conflicts: ScheduleConflict[] = [];
        const seen = new Set<string>();
        const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

        for (const v of violations) {
            const key = `shift-${Number(v.event.id)}-${v.resourceId}-${v.dayOfWeek}`;
            if (seen.has(key)) continue;
            seen.add(key);

            // Create a synthetic event representing the shift window
            const shiftEvent = new ScheduledEventData();
            shiftEvent.id = -2 as any;
            shiftEvent.name = v.shiftWindows.length > 0
                ? `Shift: ${v.shiftWindows.join(' / ')}`
                : `No shift on ${dayNames[v.dayOfWeek]}`;
            shiftEvent.startDateTime = v.event.startDateTime;
            shiftEvent.endDateTime = v.event.endDateTime;

            conflicts.push({
                eventA: v.event,
                eventB: shiftEvent,
                type: 'shift',
                entityId: v.resourceId,
                entityName: v.resourceName,
                overlapMinutes: 0,
                shiftDescription: v.description
            });
        }

        return conflicts;
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
