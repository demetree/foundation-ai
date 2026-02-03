import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { OnCallScheduleService, OnCallScheduleData } from '../../alerting-data-services/on-call-schedule.service';
import { ScheduleLayerService, ScheduleLayerData } from '../../alerting-data-services/schedule-layer.service';
import { ScheduleLayerMemberService, ScheduleLayerMemberData } from '../../alerting-data-services/schedule-layer-member.service';
import { ScheduleOverrideService, ScheduleOverrideData, ScheduleOverrideQueryParameters } from '../../alerting-data-services/schedule-override.service';
import { EscalationPolicyService, EscalationPolicyData } from '../../alerting-data-services/escalation-policy.service';
import { EscalationRuleService, EscalationRuleData } from '../../alerting-data-services/escalation-rule.service';
import { AlertService } from '../../services/alert.service';
import { AuthService } from '../../services/auth.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';

/**
 * Shift information for display
 */
interface ShiftInfo {
    scheduleName: string;
    layerName: string;
    startTime: Date;
    endTime: Date;
    isCurrent: boolean;
}

/**
 * Override information for display
 */
interface OverrideInfo {
    scheduleName: string;
    startTime: Date;
    endTime: Date;
    reason: string | null;
    originalUserName: string | null;
    isUserTakingOver: boolean; // true = user is replacement, false = user is being replaced
}

/**
 * Policy info for display
 */
interface PolicyInfo {
    policyName: string;
    serviceName: string | null;
    level: number;
}

/**
 * My Shift Component
 * 
 * Shows the current user's on-call schedule status:
 * - Current on-call status
 * - Upcoming shifts
 * - Active overrides
 * - Escalation policy memberships
 */
@Component({
    selector: 'app-my-shift',
    templateUrl: './my-shift.component.html',
    styleUrls: ['./my-shift.component.scss']
})
export class MyShiftComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Loading states
    isLoading = true;
    errorMessage: string | null = null;

    // Current user
    currentUserGuid: string = '';
    currentUserName: string = '';

    // On-call status
    isOnCallNow = false;
    currentShift: ShiftInfo | null = null;

    // Upcoming shifts (next 7 days)
    upcomingShifts: ShiftInfo[] = [];

    // Active overrides affecting user
    activeOverrides: OverrideInfo[] = [];

    // Escalation policies
    myPolicies: PolicyInfo[] = [];

    // User lookup for display names
    userMap: Map<string, AlertingUser> = new Map();

    constructor(
        private router: Router,
        private onCallScheduleService: OnCallScheduleService,
        private scheduleLayerService: ScheduleLayerService,
        private scheduleLayerMemberService: ScheduleLayerMemberService,
        private scheduleOverrideService: ScheduleOverrideService,
        private escalationPolicyService: EscalationPolicyService,
        private escalationRuleService: EscalationRuleService,
        private alertService: AlertService,
        private authService: AuthService,
        private alertingUserService: AlertingUserService
    ) { }

    ngOnInit(): void {
        this.currentUserGuid = this.authService.currentUser?.id || '';
        this.currentUserName = this.authService.currentUser?.fullName || this.authService.currentUser?.userName || 'Unknown';
        this.loadData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Load all data
     */
    private loadData(): void {
        this.isLoading = true;
        this.errorMessage = null;

        // Load users first for name resolution
        this.alertingUserService.getUsers()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (users) => {
                    this.userMap.clear();
                    for (const user of users) {
                        this.userMap.set(user.objectGuid, user);
                    }
                    this.loadSchedulesAndOverrides();
                },
                error: (error) => {
                    this.alertService.showHttpErrorMessage('Error loading users', error);
                    // Continue anyway
                    this.loadSchedulesAndOverrides();
                }
            });
    }

    /**
     * Load schedules where user is a member
     */
    private loadSchedulesAndOverrides(): void {
        forkJoin({
            schedules: this.onCallScheduleService.GetOnCallScheduleList({ active: true, deleted: false, includeRelations: true }),
            overrides: this.scheduleOverrideService.GetScheduleOverrideList({ active: true, deleted: false }),
            policies: this.escalationPolicyService.GetEscalationPolicyList({ active: true, deleted: false }),
            policyRules: this.escalationRuleService.GetEscalationRuleList({ active: true, deleted: false })
        }).pipe(takeUntil(this.destroy$))
            .subscribe({
                next: async (result) => {
                    await this.processSchedules(result.schedules);
                    this.processOverrides(result.overrides);
                    await this.processPolicies(result.policies, result.policyRules);
                    this.isLoading = false;
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load schedule data';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });
    }

    /**
     * Process schedules to find user's shifts
     */
    private async processSchedules(schedules: OnCallScheduleData[]): Promise<void> {
        const now = new Date();
        const sevenDaysLater = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

        this.upcomingShifts = [];
        this.currentShift = null;
        this.isOnCallNow = false;

        for (const schedule of schedules) {
            try {
                const layers = await schedule.ScheduleLayers;

                for (const layer of layers) {
                    const members = await layer.ScheduleLayerMembers;

                    // Check if current user is a member
                    const userMember = members.find(m => m.securityUserObjectGuid === this.currentUserGuid);
                    if (!userMember) continue;

                    // Calculate shifts for this layer
                    const shifts = this.calculateShiftsForLayer(
                        schedule.name,
                        layer,
                        members,
                        userMember.position as number,
                        now,
                        sevenDaysLater
                    );

                    for (const shift of shifts) {
                        if (shift.isCurrent) {
                            this.isOnCallNow = true;
                            this.currentShift = shift;
                        } else {
                            this.upcomingShifts.push(shift);
                        }
                    }
                }
            } catch (error) {
                console.error('Error loading schedule layers', error);
            }
        }

        // Sort upcoming shifts by start time
        this.upcomingShifts.sort((a, b) => a.startTime.getTime() - b.startTime.getTime());
    }

    /**
     * Calculate shifts for a layer based on rotation
     */
    private calculateShiftsForLayer(
        scheduleName: string,
        layer: ScheduleLayerData,
        members: ScheduleLayerMemberData[],
        userPosition: number,
        startDate: Date,
        endDate: Date
    ): ShiftInfo[] {
        const shifts: ShiftInfo[] = [];
        const now = new Date();

        // Parse layer parameters
        const rotationStart = new Date(layer.rotationStart);
        const rotationDays = Number(layer.rotationDays);
        const handoffTimeParts = layer.handoffTime.split(':');
        const handoffHour = parseInt(handoffTimeParts[0], 10);
        const handoffMinute = parseInt(handoffTimeParts[1] || '0', 10);

        if (rotationDays <= 0 || members.length === 0) return shifts;

        // Calculate rotation cycle length (total days for all members)
        const cycleLengthDays = rotationDays * members.length;

        // Find the start of the first rotation cycle before our window
        let cycleStart = new Date(rotationStart);
        cycleStart.setHours(handoffHour, handoffMinute, 0, 0);

        // Move cycle start to before our window start
        while (cycleStart > startDate) {
            cycleStart = new Date(cycleStart.getTime() - cycleLengthDays * 24 * 60 * 60 * 1000);
        }

        // Iterate through cycles to find user's shifts
        let currentCycleStart = cycleStart;
        const maxIterations = 100; // Safety limit
        let iterations = 0;

        while (currentCycleStart < endDate && iterations < maxIterations) {
            iterations++;

            // Calculate when user's shift starts in this cycle
            // User position is 1-based, so subtract 1
            const userShiftOffsetDays = (userPosition - 1) * rotationDays;
            const userShiftStart = new Date(currentCycleStart.getTime() + userShiftOffsetDays * 24 * 60 * 60 * 1000);
            const userShiftEnd = new Date(userShiftStart.getTime() + rotationDays * 24 * 60 * 60 * 1000);

            // Check if this shift overlaps our window
            if (userShiftEnd > startDate && userShiftStart < endDate) {
                const isCurrent = now >= userShiftStart && now < userShiftEnd;

                shifts.push({
                    scheduleName: scheduleName,
                    layerName: layer.name,
                    startTime: userShiftStart,
                    endTime: userShiftEnd,
                    isCurrent: isCurrent
                });
            }

            // Move to next cycle
            currentCycleStart = new Date(currentCycleStart.getTime() + cycleLengthDays * 24 * 60 * 60 * 1000);
        }

        return shifts;
    }

    /**
     * Process overrides affecting user
     */
    private processOverrides(overrides: ScheduleOverrideData[]): void {
        const now = new Date();
        this.activeOverrides = [];

        for (const override of overrides) {
            const startTime = new Date(override.startDateTime);
            const endTime = new Date(override.endDateTime);

            // Only show active/future overrides
            if (endTime < now) continue;

            const isUserTakingOver = override.replacementUserObjectGuid === this.currentUserGuid;
            const isUserBeingReplaced = override.originalUserObjectGuid === this.currentUserGuid;

            if (isUserTakingOver || isUserBeingReplaced) {
                const otherUserGuid = isUserTakingOver
                    ? override.originalUserObjectGuid
                    : override.replacementUserObjectGuid;

                this.activeOverrides.push({
                    scheduleName: `Schedule #${override.onCallScheduleId}`, // We'll enhance this later
                    startTime: startTime,
                    endTime: endTime,
                    reason: override.reason,
                    originalUserName: otherUserGuid ? this.getUserName(otherUserGuid) : null,
                    isUserTakingOver: isUserTakingOver
                });
            }
        }

        // Sort by start time
        this.activeOverrides.sort((a, b) => a.startTime.getTime() - b.startTime.getTime());
    }

    /**
     * Process escalation policies where user is referenced
     */
    private async processPolicies(policies: EscalationPolicyData[], rules: EscalationRuleData[]): Promise<void> {
        this.myPolicies = [];

        // Find rules that reference the current user (targetType = 'User' and targetObjectGuid = userGuid)
        const userRules = rules.filter(r => r.targetType === 'User' && r.targetObjectGuid === this.currentUserGuid);

        for (const rule of userRules) {
            const policy = policies.find(p => p.id === rule.escalationPolicyId);
            if (policy) {
                this.myPolicies.push({
                    policyName: policy.name,
                    serviceName: null, // Could load service if needed
                    level: Number(rule.ruleOrder)
                });
            }
        }

        // Sort by policy name then level
        this.myPolicies.sort((a, b) => {
            const nameCompare = a.policyName.localeCompare(b.policyName);
            if (nameCompare !== 0) return nameCompare;
            return a.level - b.level;
        });
    }

    // ===== Display Helpers =====

    getUserName(userGuid: string): string {
        const user = this.userMap.get(userGuid);
        return user?.displayName || user?.accountName || 'Unknown User';
    }

    formatTime(date: Date): string {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    formatDate(date: Date): string {
        return date.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' });
    }

    formatDateTime(date: Date): string {
        return `${this.formatDate(date)} • ${this.formatTime(date)}`;
    }

    getShiftTimeRange(shift: ShiftInfo): string {
        if (this.isSameDay(shift.startTime, shift.endTime)) {
            return `${this.formatTime(shift.startTime)} - ${this.formatTime(shift.endTime)}`;
        }
        return `${this.formatDate(shift.startTime)} ${this.formatTime(shift.startTime)} - ${this.formatDate(shift.endTime)} ${this.formatTime(shift.endTime)}`;
    }

    private isSameDay(d1: Date, d2: Date): boolean {
        return d1.getFullYear() === d2.getFullYear() &&
            d1.getMonth() === d2.getMonth() &&
            d1.getDate() === d2.getDate();
    }

    getTimeUntilEnd(): string {
        if (!this.currentShift) return '';
        const now = new Date();
        const diffMs = this.currentShift.endTime.getTime() - now.getTime();
        const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
        const diffMins = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

        if (diffHours > 0) {
            return `${diffHours}h ${diffMins}m remaining`;
        }
        return `${diffMins}m remaining`;
    }

    // ===== Navigation =====

    goBack(): void {
        this.router.navigate(['/respond']);
    }

    goToResponderConsole(): void {
        this.router.navigate(['/respond']);
    }

    goToDashboard(): void {
        this.router.navigate(['/incident-dashboard']);
    }

    refresh(): void {
        this.loadData();
    }
}
