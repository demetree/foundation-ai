/**
 * VolunteerSuggestionService
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Client-side matching service that scores and ranks volunteers for an event assignment.
 *
 * Scoring criteria (100-point scale):
 *   - Skills / interests keyword match  (0-30 pts)
 *   - Availability preference match     (0-20 pts)
 *   - Recent activity                   (0-15 pts)
 *   - Compliance status                 (0-15 pts)
 *   - Workload balance                  (0-20 pts)
 */

import { Injectable } from '@angular/core';
import { VolunteerProfileData } from '../scheduler-data-services/volunteer-profile.service';
import { EventResourceAssignmentData } from '../scheduler-data-services/event-resource-assignment.service';

export interface VolunteerSuggestion {
    volunteer: VolunteerProfileData;
    score: number;
    matchReasons: MatchReason[];
}

export interface MatchReason {
    label: string;
    icon: string;
    type: 'positive' | 'neutral' | 'negative';
}

export interface SuggestionContext {
    /** Name of the event being staffed */
    eventName?: string;
    /** Description / notes for the event */
    eventDescription?: string;
    /** Day of week (0=Sun, 6=Sat) */
    eventDayOfWeek?: number;
    /** IDs of volunteers already assigned to this event */
    alreadyAssignedResourceIds?: (number | bigint)[];
    /** All current assignments across the system (for workload calculation) */
    allAssignments?: EventResourceAssignmentData[];
}

@Injectable({ providedIn: 'root' })
export class VolunteerSuggestionService {

    /**
     * Rank volunteers for an event context.
     * Returns top N suggestions sorted by descending score.
     */
    getSuggestions(
        volunteers: VolunteerProfileData[],
        context: SuggestionContext,
        topN: number = 5
    ): VolunteerSuggestion[] {
        const now = new Date();

        const scored = volunteers
            .filter(v => v.active !== false)
            .filter(v => {
                // Exclude already-assigned volunteers
                if (context.alreadyAssignedResourceIds?.length) {
                    return !context.alreadyAssignedResourceIds.includes(Number(v.resourceId));
                }
                return true;
            })
            .map(v => this.scoreVolunteer(v, context, now));

        scored.sort((a, b) => b.score - a.score);
        return scored.slice(0, topN);
    }

    private scoreVolunteer(
        volunteer: VolunteerProfileData,
        context: SuggestionContext,
        now: Date
    ): VolunteerSuggestion {
        let score = 0;
        const reasons: MatchReason[] = [];

        // 1. Skills / Interests keyword match (0-30 pts)
        score += this.scoreSkillsMatch(volunteer, context, reasons);

        // 2. Availability preference (0-20 pts)
        score += this.scoreAvailability(volunteer, context, reasons);

        // 3. Recent activity (0-15 pts)
        score += this.scoreRecency(volunteer, now, reasons);

        // 4. Compliance (0-15 pts)
        score += this.scoreCompliance(volunteer, now, reasons);

        // 5. Workload balance (0-20 pts)
        score += this.scoreWorkload(volunteer, context, reasons);

        return {
            volunteer,
            score: Math.round(Math.min(100, Math.max(0, score))),
            matchReasons: reasons
        };
    }

    // --- Scoring Functions ---

    private scoreSkillsMatch(
        v: VolunteerProfileData,
        ctx: SuggestionContext,
        reasons: MatchReason[]
    ): number {
        if (!ctx.eventName && !ctx.eventDescription) return 10; // neutral

        const volunteerText = [
            v.interestsAndSkillsNotes || '',
            v.availabilityPreferences || ''
        ].join(' ').toLowerCase();

        if (!volunteerText.trim()) {
            reasons.push({ label: 'No skills listed', icon: 'fa-solid fa-circle-question', type: 'neutral' });
            return 10;
        }

        const eventText = [ctx.eventName || '', ctx.eventDescription || ''].join(' ').toLowerCase();
        const keywords = this.extractKeywords(eventText);

        const matches = keywords.filter(kw => volunteerText.includes(kw));
        const matchRatio = keywords.length > 0 ? matches.length / keywords.length : 0;

        if (matchRatio > 0.3) {
            reasons.push({ label: `Skills match: ${matches.slice(0, 3).join(', ')}`, icon: 'fa-solid fa-bullseye', type: 'positive' });
            return Math.min(30, Math.round(matchRatio * 30));
        } else if (matchRatio > 0) {
            reasons.push({ label: 'Partial skills match', icon: 'fa-solid fa-circle-half-stroke', type: 'neutral' });
            return Math.round(matchRatio * 30);
        }

        return 5;
    }

    private scoreAvailability(
        v: VolunteerProfileData,
        ctx: SuggestionContext,
        reasons: MatchReason[]
    ): number {
        const prefs = (v.availabilityPreferences || '').toLowerCase();
        if (!prefs) return 10; // neutral — no preferences listed

        if (ctx.eventDayOfWeek !== undefined && ctx.eventDayOfWeek !== null) {
            const dayNames = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
            const dayShort = ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat'];
            const dayName = dayNames[ctx.eventDayOfWeek];
            const dayAbbr = dayShort[ctx.eventDayOfWeek];

            if (prefs.includes(dayName) || prefs.includes(dayAbbr)) {
                reasons.push({ label: `Available ${dayName}s`, icon: 'fa-solid fa-calendar-check', type: 'positive' });
                return 20;
            }
        }

        // General availability keywords
        if (prefs.includes('anytime') || prefs.includes('flexible') || prefs.includes('any day')) {
            reasons.push({ label: 'Flexible availability', icon: 'fa-solid fa-calendar-check', type: 'positive' });
            return 18;
        }

        return 10;
    }

    private scoreRecency(
        v: VolunteerProfileData,
        now: Date,
        reasons: MatchReason[]
    ): number {
        if (!v.lastActivityDate) {
            reasons.push({ label: 'No recent activity', icon: 'fa-solid fa-user-clock', type: 'neutral' });
            return 5;
        }

        const daysSinceActivity = Math.floor((now.getTime() - new Date(v.lastActivityDate).getTime()) / (1000 * 60 * 60 * 24));

        if (daysSinceActivity <= 14) {
            reasons.push({ label: 'Recently active', icon: 'fa-solid fa-bolt', type: 'positive' });
            return 15;
        } else if (daysSinceActivity <= 60) {
            return 10;
        } else if (daysSinceActivity <= 90) {
            reasons.push({ label: 'Not active in 60+ days', icon: 'fa-solid fa-user-clock', type: 'neutral' });
            return 5;
        }

        reasons.push({ label: 'Inactive 90+ days', icon: 'fa-solid fa-user-slash', type: 'negative' });
        return 0;
    }

    private scoreCompliance(
        v: VolunteerProfileData,
        now: Date,
        reasons: MatchReason[]
    ): number {
        let complianceScore = 15;

        // BG check expired?
        if (v.backgroundCheckExpiry) {
            const expiry = new Date(v.backgroundCheckExpiry);
            if (expiry < now) {
                reasons.push({ label: 'BG check expired', icon: 'fa-solid fa-shield-xmark', type: 'negative' });
                complianceScore -= 15; // disqualify
            } else {
                const thirtyDays = new Date(now.getTime() + 30 * 24 * 60 * 60 * 1000);
                if (expiry <= thirtyDays) {
                    reasons.push({ label: 'BG check expiring soon', icon: 'fa-solid fa-shield-halved', type: 'neutral' });
                    complianceScore -= 5;
                }
            }
        } else if (v.backgroundCheckCompleted === false) {
            reasons.push({ label: 'No BG check', icon: 'fa-solid fa-shield-xmark', type: 'negative' });
            complianceScore -= 10;
        }

        return Math.max(0, complianceScore);
    }

    private scoreWorkload(
        v: VolunteerProfileData,
        ctx: SuggestionContext,
        reasons: MatchReason[]
    ): number {
        if (!ctx.allAssignments || ctx.allAssignments.length === 0) return 15; // neutral

        const volunteerAssignments = ctx.allAssignments.filter(
            a => Number(a.resourceId) === Number(v.resourceId) && a.active !== false
        );

        const count = volunteerAssignments.length;

        if (count === 0) {
            reasons.push({ label: 'No current assignments', icon: 'fa-solid fa-star', type: 'positive' });
            return 20;
        } else if (count <= 2) {
            reasons.push({ label: `${count} assignment${count > 1 ? 's' : ''}`, icon: 'fa-solid fa-check', type: 'positive' });
            return 15;
        } else if (count <= 5) {
            return 10;
        }

        reasons.push({ label: `${count} assignments (heavy load)`, icon: 'fa-solid fa-weight-hanging', type: 'negative' });
        return 5;
    }

    // --- Helpers ---

    private extractKeywords(text: string): string[] {
        const stopWords = new Set([
            'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for',
            'of', 'with', 'by', 'is', 'are', 'was', 'be', 'have', 'has', 'this',
            'that', 'it', 'from', 'as', 'do', 'not', 'we', 'our', 'will', 'can'
        ]);

        return text
            .replace(/[^\w\s]/g, ' ')
            .split(/\s+/)
            .filter(w => w.length >= 3 && !stopWords.has(w));
    }
}
