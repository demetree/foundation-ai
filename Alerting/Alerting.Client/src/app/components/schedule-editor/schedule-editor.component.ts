import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

import { OnCallScheduleService, OnCallScheduleData, OnCallScheduleSubmitData } from '../../alerting-data-services/on-call-schedule.service';
import { ScheduleLayerService, ScheduleLayerData, ScheduleLayerSubmitData } from '../../alerting-data-services/schedule-layer.service';
import { ScheduleLayerMemberService, ScheduleLayerMemberData, ScheduleLayerMemberSubmitData } from '../../alerting-data-services/schedule-layer-member.service';
import { ScheduleOverrideService, ScheduleOverrideData, ScheduleOverrideSubmitData } from '../../alerting-data-services/schedule-override.service';
import { ScheduleOverrideTypeService, ScheduleOverrideTypeData } from '../../alerting-data-services/schedule-override-type.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';
import { AlertService } from '../../services/alert.service';

/**
 * Local model for editing layers (includes UI state)
 */
export interface EditableLayer {
    id: number | null;
    name: string;
    layerLevel: number;
    rotationStart: Date;
    rotationDays: number;
    handoffTime: string;
    members: EditableMember[];
    isNew: boolean;
    isModified: boolean;
    isExpanded: boolean;
    originalData: ScheduleLayerData | null;
}

/**
 * Local model for editing layer members
 */
export interface EditableMember {
    id: number | null;
    position: number;
    securityUserObjectGuid: string;
    displayName: string;
    isNew: boolean;
    originalData: ScheduleLayerMemberData | null;
}

/**
 * Timeline span for visualization
 */
export interface OnCallSpan {
    userId: string;
    userName: string;
    startTime: Date;
    endTime: Date;
    layerId: number;
    layerName: string;
    layerLevel: number;
    color: string;
}

/**
 * Local model for editing overrides
 */
export interface EditableOverride {
    id: number | null;
    scheduleLayerId: number | null;
    scheduleOverrideTypeId: number;
    startDateTime: Date;
    endDateTime: Date;
    originalUserObjectGuid: string | null;
    replacementUserObjectGuid: string | null;
    reason: string;
    isNew: boolean;
    isModified: boolean;
    originalData: ScheduleOverrideData | null;
}

/**
 * Represents a period with no on-call coverage
 */
export interface GapPeriod {
    startTime: Date;
    endTime: Date;
}

/**
 * Timeline span for override visualization
 */
export interface OverrideSpan {
    overrideId: number | null;
    startTime: Date;
    endTime: Date;
    overrideType: string;
    originalUserName: string | null;
    replacementUserName: string | null;
    reason: string;
    isPast: boolean;
    isActive: boolean;
}

@Component({
    selector: 'app-schedule-editor',
    templateUrl: './schedule-editor.component.html',
    styleUrls: ['./schedule-editor.component.scss']
})
export class ScheduleEditorComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Route
    scheduleId: number | null = null;
    isNewSchedule = false;

    // Loading states
    isLoading = true;
    isSaving = false;
    errorMessage: string | null = null;

    // Schedule data
    schedule: OnCallScheduleData | null = null;

    // Form fields for schedule metadata
    formName = '';
    formDescription = '';
    formTimeZoneId = 'UTC';
    formActive = true;

    // Layers
    layers: EditableLayer[] = [];

    // Available users
    users: AlertingUser[] = [];
    usersLoading = false;

    // Timeline
    timelineStartDate: Date = new Date();
    timelineEndDate: Date = new Date();
    timelineDays: Date[] = [];
    timelineSpans: OnCallSpan[] = [];
    today: Date = new Date();

    // Gap detection
    coverageGaps: GapPeriod[] = [];
    hasGaps = false;

    // Overrides
    overrides: EditableOverride[] = [];
    overrideTypes: ScheduleOverrideTypeData[] = [];
    showOverrideModal = false;
    editingOverride: EditableOverride | null = null;
    deletedOverrideIds: number[] = [];
    timelineOverrideSpans: OverrideSpan[] = [];

    // Track unsaved changes
    hasUnsavedChanges = false;

    // Common timezones
    commonTimezones = [
        { id: 'UTC', label: 'UTC' },
        { id: 'America/New_York', label: 'Eastern Time (US)' },
        { id: 'America/Chicago', label: 'Central Time (US)' },
        { id: 'America/Denver', label: 'Mountain Time (US)' },
        { id: 'America/Los_Angeles', label: 'Pacific Time (US)' },
        { id: 'America/Toronto', label: 'Eastern Time (Canada)' },
        { id: 'America/St_Johns', label: 'Newfoundland Time' },
        { id: 'Europe/London', label: 'London (GMT/BST)' },
        { id: 'Europe/Paris', label: 'Paris (CET)' },
        { id: 'Europe/Berlin', label: 'Berlin (CET)' },
        { id: 'Asia/Tokyo', label: 'Tokyo (JST)' },
        { id: 'Asia/Shanghai', label: 'Shanghai (CST)' },
        { id: 'Australia/Sydney', label: 'Sydney (AEST)' }
    ];

    // Color palette for users
    private userColors: Map<string, string> = new Map();
    private colorPalette = [
        '#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6',
        '#06b6d4', '#ec4899', '#84cc16', '#f97316', '#6366f1'
    ];

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private scheduleService: OnCallScheduleService,
        private layerService: ScheduleLayerService,
        private memberService: ScheduleLayerMemberService,
        private overrideService: ScheduleOverrideService,
        private overrideTypeService: ScheduleOverrideTypeService,
        private alertingUserService: AlertingUserService,
        private alertService: AlertService
    ) { }

    ngOnInit(): void {
        // Initialize timeline range (next 14 days)
        this.initializeTimeline();

        // Get schedule ID from route
        const idParam = this.route.snapshot.paramMap.get('id');

        if (idParam === 'new') {
            this.isNewSchedule = true;
            this.isLoading = false;
            this.loadUsers();
        } else if (idParam) {
            this.scheduleId = parseInt(idParam, 10);
            this.loadScheduleData();
        } else {
            this.errorMessage = 'Invalid schedule ID';
            this.isLoading = false;
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initializeTimeline(): void {
        const now = new Date();
        this.timelineStartDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        this.timelineEndDate = new Date(this.timelineStartDate);
        this.timelineEndDate.setDate(this.timelineEndDate.getDate() + 14);

        // Generate day labels
        this.timelineDays = [];
        const current = new Date(this.timelineStartDate);
        while (current < this.timelineEndDate) {
            this.timelineDays.push(new Date(current));
            current.setDate(current.getDate() + 1);
        }
    }

    private loadScheduleData(): void {
        this.isLoading = true;
        this.errorMessage = null;

        forkJoin({
            schedule: this.scheduleService.GetOnCallSchedule(this.scheduleId!, true),
            users: this.alertingUserService.getUsers()
        }).pipe(takeUntil(this.destroy$))
            .subscribe({
                next: async ({ schedule, users }) => {
                    this.schedule = schedule;
                    this.users = users;

                    // Populate form fields
                    this.formName = schedule.name;
                    this.formDescription = schedule.description || '';
                    this.formTimeZoneId = schedule.timeZoneId;
                    this.formActive = schedule.active;

                    // Load layers
                    await this.loadLayers();

                    // Load overrides
                    await this.loadOverrides();

                    this.isLoading = false;
                    this.updateTimeline();
                },
                error: (err) => {
                    console.error('Error loading schedule:', err);
                    this.errorMessage = 'Failed to load schedule. Please try again.';
                    this.isLoading = false;
                }
            });
    }

    private loadUsers(): void {
        this.usersLoading = true;
        this.alertingUserService.getUsers()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (users) => {
                    this.users = users;
                    this.usersLoading = false;
                },
                error: (err) => {
                    console.error('Error loading users:', err);
                    this.usersLoading = false;
                }
            });
    }

    private async loadLayers(): Promise<void> {
        if (!this.schedule) return;

        try {
            const layersData = await this.schedule.ScheduleLayers;
            this.layers = [];

            for (const layerData of layersData.sort((a, b) => Number(a.layerLevel) - Number(b.layerLevel))) {
                const members = await layerData.ScheduleLayerMembers;
                const editableMembers: EditableMember[] = members
                    .sort((a, b) => Number(a.position) - Number(b.position))
                    .map(m => ({
                        id: Number(m.id),
                        position: Number(m.position),
                        securityUserObjectGuid: m.securityUserObjectGuid,
                        displayName: this.getUserDisplayName(m.securityUserObjectGuid),
                        isNew: false,
                        originalData: m
                    }));

                this.layers.push({
                    id: Number(layerData.id),
                    name: layerData.name,
                    layerLevel: Number(layerData.layerLevel),
                    rotationStart: new Date(layerData.rotationStart),
                    rotationDays: Number(layerData.rotationDays),
                    handoffTime: layerData.handoffTime,
                    members: editableMembers,
                    isNew: false,
                    isModified: false,
                    isExpanded: true,
                    originalData: layerData
                });
            }
        } catch (err) {
            console.error('Error loading layers:', err);
        }
    }

    // ==================== User Helpers ====================

    getUserDisplayName(guid: string): string {
        const user = this.users.find(u => u.objectGuid === guid);
        return user?.displayName || guid.substring(0, 8) + '...';
    }

    getUserColor(guid: string): string {
        if (!this.userColors.has(guid)) {
            const colorIndex = this.userColors.size % this.colorPalette.length;
            this.userColors.set(guid, this.colorPalette[colorIndex]);
        }
        return this.userColors.get(guid)!;
    }

    getUserInitials(guid: string): string {
        const user = this.users.find(u => u.objectGuid === guid);
        if (user) {
            const first = user.firstName?.[0] || '';
            const last = user.lastName?.[0] || '';
            return (first + last).toUpperCase() || '??';
        }
        return '??';
    }

    // ==================== Layer Management ====================

    addLayer(): void {
        const newLayer: EditableLayer = {
            id: null,
            name: `Layer ${this.layers.length + 1}`,
            layerLevel: this.layers.length + 1,
            rotationStart: new Date(),
            rotationDays: 7,
            handoffTime: '09:00',
            members: [],
            isNew: true,
            isModified: false,
            isExpanded: true,
            originalData: null
        };
        this.layers.push(newLayer);
        this.markUnsavedChanges();
    }

    removeLayer(index: number): void {
        if (confirm('Are you sure you want to remove this layer?')) {
            this.layers.splice(index, 1);
            // Reorder remaining layers
            this.layers.forEach((layer, i) => {
                layer.layerLevel = i + 1;
            });
            this.markUnsavedChanges();
            this.updateTimeline();
        }
    }

    toggleLayerExpansion(layer: EditableLayer): void {
        layer.isExpanded = !layer.isExpanded;
    }

    onLayerDrop(event: CdkDragDrop<EditableLayer[]>): void {
        moveItemInArray(this.layers, event.previousIndex, event.currentIndex);
        // Update layer levels
        this.layers.forEach((layer, i) => {
            layer.layerLevel = i + 1;
            layer.isModified = true;
        });
        this.markUnsavedChanges();
        this.updateTimeline();
    }

    onLayerFieldChange(layer: EditableLayer): void {
        layer.isModified = true;
        this.markUnsavedChanges();
        this.updateTimeline();
    }

    // ==================== Member Management ====================

    addMember(layer: EditableLayer): void {
        const newMember: EditableMember = {
            id: null,
            position: layer.members.length,
            securityUserObjectGuid: '',
            displayName: '',
            isNew: true,
            originalData: null
        };
        layer.members.push(newMember);
        layer.isModified = true;
        this.markUnsavedChanges();
    }

    removeMember(layer: EditableLayer, memberIndex: number): void {
        layer.members.splice(memberIndex, 1);
        // Reorder positions
        layer.members.forEach((m, i) => m.position = i);
        layer.isModified = true;
        this.markUnsavedChanges();
        this.updateTimeline();
    }

    onMemberDrop(layer: EditableLayer, event: CdkDragDrop<EditableMember[]>): void {
        moveItemInArray(layer.members, event.previousIndex, event.currentIndex);
        // Update positions
        layer.members.forEach((m, i) => m.position = i);
        layer.isModified = true;
        this.markUnsavedChanges();
        this.updateTimeline();
    }

    onMemberUserChange(layer: EditableLayer, member: EditableMember): void {
        member.displayName = this.getUserDisplayName(member.securityUserObjectGuid);
        layer.isModified = true;
        this.markUnsavedChanges();
        this.updateTimeline();
    }

    // ==================== Timeline Calculation ====================

    updateTimeline(): void {
        this.timelineSpans = [];

        for (const layer of this.layers) {
            if (layer.members.length === 0) continue;

            const spans = this.calculateLayerSpans(layer);
            this.timelineSpans.push(...spans);
        }

        // Detect coverage gaps after updating timeline
        this.detectCoverageGaps();

        // Calculate override spans for timeline visualization
        this.calculateTimelineOverrideSpans();
    }

    /**
     * Calculates override spans that fall within the timeline window
     */
    private calculateTimelineOverrideSpans(): void {
        this.timelineOverrideSpans = [];
        const now = new Date();

        for (const override of this.overrides) {
            // Check if override overlaps with timeline window
            const overrideStart = new Date(override.startDateTime);
            const overrideEnd = new Date(override.endDateTime);

            if (overrideEnd < this.timelineStartDate || overrideStart > this.timelineEndDate) {
                continue; // Outside timeline window
            }

            // Clip to timeline window
            const spanStart = new Date(Math.max(overrideStart.getTime(), this.timelineStartDate.getTime()));
            const spanEnd = new Date(Math.min(overrideEnd.getTime(), this.timelineEndDate.getTime()));

            const isPast = overrideEnd < now;
            const isActive = overrideStart <= now && overrideEnd >= now;

            this.timelineOverrideSpans.push({
                overrideId: override.id,
                startTime: spanStart,
                endTime: spanEnd,
                overrideType: this.getOverrideTypeName(override.scheduleOverrideTypeId),
                originalUserName: override.originalUserObjectGuid
                    ? this.getUserName(override.originalUserObjectGuid)
                    : null,
                replacementUserName: override.replacementUserObjectGuid
                    ? this.getUserName(override.replacementUserObjectGuid)
                    : null,
                reason: override.reason,
                isPast,
                isActive
            });
        }

        // Sort by start time
        this.timelineOverrideSpans.sort((a, b) => a.startTime.getTime() - b.startTime.getTime());
    }

    getOverrideSpanStyle(span: OverrideSpan): { [key: string]: string } {
        const totalMs = this.timelineEndDate.getTime() - this.timelineStartDate.getTime();
        const startOffset = span.startTime.getTime() - this.timelineStartDate.getTime();
        const spanWidth = span.endTime.getTime() - span.startTime.getTime();

        return {
            left: `${(startOffset / totalMs) * 100}%`,
            width: `${(spanWidth / totalMs) * 100}%`
        };
    }

    getOverrideTooltip(span: OverrideSpan): string {
        let tooltip = `${span.overrideType}`;
        if (span.originalUserName && span.replacementUserName) {
            tooltip += `: ${span.originalUserName} → ${span.replacementUserName}`;
        } else if (span.replacementUserName) {
            tooltip += `: ${span.replacementUserName}`;
        }
        if (span.reason) {
            tooltip += ` (${span.reason})`;
        }
        return tooltip;
    }


    /**
     * Detects periods where no layer has anyone on-call
     */
    private detectCoverageGaps(): void {
        this.coverageGaps = [];

        // If no layers, entire timeline is a gap
        if (this.layers.length === 0 || this.timelineSpans.length === 0) {
            if (this.layers.length === 0) {
                this.coverageGaps.push({
                    startTime: this.timelineStartDate,
                    endTime: this.timelineEndDate
                });
            }
            this.hasGaps = this.coverageGaps.length > 0;
            return;
        }

        // Build a timeline of covered periods by merging all spans
        // Sort spans by start time
        const sortedSpans = [...this.timelineSpans].sort((a, b) =>
            a.startTime.getTime() - b.startTime.getTime()
        );

        // Merge overlapping spans to find covered periods
        const coveredPeriods: { start: Date; end: Date }[] = [];

        for (const span of sortedSpans) {
            if (coveredPeriods.length === 0) {
                coveredPeriods.push({ start: span.startTime, end: span.endTime });
            } else {
                const last = coveredPeriods[coveredPeriods.length - 1];
                if (span.startTime <= last.end) {
                    // Overlapping or adjacent, extend the period
                    last.end = new Date(Math.max(last.end.getTime(), span.endTime.getTime()));
                } else {
                    // New period
                    coveredPeriods.push({ start: span.startTime, end: span.endTime });
                }
            }
        }

        // Find gaps between covered periods
        let currentTime = this.timelineStartDate;

        for (const period of coveredPeriods) {
            if (period.start > currentTime) {
                // Gap before this period
                this.coverageGaps.push({
                    startTime: new Date(currentTime),
                    endTime: new Date(period.start)
                });
            }
            currentTime = new Date(Math.max(currentTime.getTime(), period.end.getTime()));
        }

        // Check for gap at the end
        if (currentTime < this.timelineEndDate) {
            this.coverageGaps.push({
                startTime: new Date(currentTime),
                endTime: new Date(this.timelineEndDate)
            });
        }

        this.hasGaps = this.coverageGaps.length > 0;
    }

    getGapStyle(gap: GapPeriod): { [key: string]: string } {
        const totalMs = this.timelineEndDate.getTime() - this.timelineStartDate.getTime();
        const startOffset = gap.startTime.getTime() - this.timelineStartDate.getTime();
        const gapWidth = gap.endTime.getTime() - gap.startTime.getTime();

        return {
            left: `${(startOffset / totalMs) * 100}%`,
            width: `${(gapWidth / totalMs) * 100}%`
        };
    }

    formatGapDuration(gap: GapPeriod): string {
        const durationMs = gap.endTime.getTime() - gap.startTime.getTime();
        const hours = Math.floor(durationMs / (1000 * 60 * 60));
        const days = Math.floor(hours / 24);

        if (days > 0) {
            const remainingHours = hours % 24;
            return remainingHours > 0 ? `${days}d ${remainingHours}h` : `${days}d`;
        }
        return `${hours}h`;
    }

    private calculateLayerSpans(layer: EditableLayer): OnCallSpan[] {
        const spans: OnCallSpan[] = [];
        const members = layer.members.filter(m => m.securityUserObjectGuid);

        if (members.length === 0) return spans;

        const rotationMs = layer.rotationDays * 24 * 60 * 60 * 1000;
        const rotationStart = new Date(layer.rotationStart);

        // Parse handoff time
        const [handoffHour, handoffMinute] = layer.handoffTime.split(':').map(Number);
        rotationStart.setHours(handoffHour, handoffMinute, 0, 0);

        // Calculate spans for the timeline window
        let currentTime = new Date(this.timelineStartDate);

        while (currentTime < this.timelineEndDate) {
            // Find which rotation period we're in
            const elapsedMs = currentTime.getTime() - rotationStart.getTime();
            const rotationIndex = Math.floor(elapsedMs / rotationMs);
            const memberIndex = ((rotationIndex % members.length) + members.length) % members.length;
            const member = members[memberIndex];

            // Calculate span boundaries
            const rotationStartTime = new Date(rotationStart.getTime() + (rotationIndex * rotationMs));
            const rotationEndTime = new Date(rotationStartTime.getTime() + rotationMs);

            // Clip to timeline window
            const spanStart = new Date(Math.max(currentTime.getTime(), this.timelineStartDate.getTime()));
            const spanEnd = new Date(Math.min(rotationEndTime.getTime(), this.timelineEndDate.getTime()));

            if (spanStart < spanEnd) {
                spans.push({
                    userId: member.securityUserObjectGuid,
                    userName: member.displayName,
                    startTime: spanStart,
                    endTime: spanEnd,
                    layerId: layer.id || 0,
                    layerName: layer.name,
                    layerLevel: layer.layerLevel,
                    color: this.getUserColor(member.securityUserObjectGuid)
                });
            }

            currentTime = rotationEndTime;
        }

        return spans;
    }

    getSpanStyle(span: OnCallSpan): { [key: string]: string } {
        const totalMs = this.timelineEndDate.getTime() - this.timelineStartDate.getTime();
        const startOffset = span.startTime.getTime() - this.timelineStartDate.getTime();
        const spanWidth = span.endTime.getTime() - span.startTime.getTime();

        return {
            left: `${(startOffset / totalMs) * 100}%`,
            width: `${(spanWidth / totalMs) * 100}%`,
            backgroundColor: span.color
        };
    }

    getSpansForLayer(layerLevel: number): OnCallSpan[] {
        return this.timelineSpans.filter(s => s.layerLevel === layerLevel);
    }

    getCurrentOnCall(): string {
        const now = new Date();
        const currentSpans = this.timelineSpans.filter(
            s => s.startTime <= now && s.endTime > now
        ).sort((a, b) => a.layerLevel - b.layerLevel);

        if (currentSpans.length > 0) {
            return currentSpans[0].userName;
        }
        return 'No one';
    }

    isWithinTimelineWindow(): boolean {
        const now = new Date();
        return now >= this.timelineStartDate && now <= this.timelineEndDate;
    }

    getUserName(userGuid: string): string {
        const user = this.users.find(u => u.objectGuid === userGuid);
        return user?.displayName || 'Unknown User';
    }

    // ==================== Save Logic ====================

    markUnsavedChanges(): void {
        this.hasUnsavedChanges = true;
    }

    async save(): Promise<void> {
        if (!this.formName.trim()) {
            this.alertService.showErrorMessage('Validation Error', 'Schedule name is required.');
            return;
        }

        this.isSaving = true;

        try {
            // Save schedule metadata
            let scheduleId = this.scheduleId;

            if (this.isNewSchedule) {
                const newSchedule = await this.createSchedule();
                scheduleId = Number(newSchedule.id);
                this.scheduleId = scheduleId;
                this.isNewSchedule = false;
            } else {
                await this.updateSchedule();
            }

            // Save layers
            await this.saveLayers(scheduleId!);

            // Save overrides
            await this.saveOverrides(scheduleId!);

            this.scheduleService.ClearAllCaches();
            this.layerService.ClearAllCaches();
            this.memberService.ClearAllCaches();
            this.overrideService.ClearAllCaches();

            this.hasUnsavedChanges = false;
            this.alertService.showSuccessMessage('Saved', 'Schedule saved successfully.');

            // Reload data
            if (!this.isNewSchedule) {
                this.loadScheduleData();
            }

        } catch (err: any) {
            console.error('Error saving schedule:', err);
            this.alertService.showHttpErrorMessage('Save Failed', err);
        } finally {
            this.isSaving = false;
        }
    }

    private async createSchedule(): Promise<OnCallScheduleData> {
        const submitData = new OnCallScheduleSubmitData();
        submitData.id = 0;
        submitData.name = this.formName.trim();
        submitData.description = this.formDescription.trim() || null;
        submitData.timeZoneId = this.formTimeZoneId;
        submitData.active = this.formActive;
        submitData.deleted = false;
        submitData.versionNumber = 0;

        return this.scheduleService.PostOnCallSchedule(submitData).toPromise() as Promise<OnCallScheduleData>;
    }

    private async updateSchedule(): Promise<void> {
        if (!this.schedule) return;

        const submitData = this.schedule.ConvertToSubmitData();
        submitData.name = this.formName.trim();
        submitData.description = this.formDescription.trim() || null;
        submitData.timeZoneId = this.formTimeZoneId;
        submitData.active = this.formActive;

        await this.scheduleService.PutOnCallSchedule(this.scheduleId!, submitData).toPromise();
    }

    private async saveLayers(scheduleId: number): Promise<void> {
        // Delete removed layers
        if (this.schedule) {
            const existingLayers = await this.schedule.ScheduleLayers;
            for (const existing of existingLayers) {
                const stillExists = this.layers.some(l => l.id === Number(existing.id));
                if (!stillExists) {
                    const submitData = existing.ConvertToSubmitData();
                    submitData.deleted = true;
                    await this.layerService.PutScheduleLayer(Number(existing.id), submitData).toPromise();
                }
            }
        }

        // Create/update layers
        for (const layer of this.layers) {
            if (layer.isNew) {
                const newLayer = await this.createLayer(scheduleId, layer);
                layer.id = Number(newLayer.id);
                layer.isNew = false;
            } else if (layer.isModified && layer.id) {
                await this.updateLayer(layer);
            }

            // Save members
            await this.saveMembers(layer);
        }
    }

    private async createLayer(scheduleId: number, layer: EditableLayer): Promise<ScheduleLayerData> {
        const submitData = new ScheduleLayerSubmitData();
        submitData.id = 0;
        submitData.onCallScheduleId = scheduleId;
        submitData.name = layer.name;
        submitData.layerLevel = layer.layerLevel;
        submitData.rotationStart = layer.rotationStart.toISOString();
        submitData.rotationDays = layer.rotationDays;
        submitData.handoffTime = layer.handoffTime;
        submitData.active = true;
        submitData.deleted = false;
        submitData.versionNumber = 0;

        return this.layerService.PostScheduleLayer(submitData).toPromise() as Promise<ScheduleLayerData>;
    }

    private async updateLayer(layer: EditableLayer): Promise<void> {
        if (!layer.originalData) return;

        const submitData = layer.originalData.ConvertToSubmitData();
        submitData.name = layer.name;
        submitData.layerLevel = layer.layerLevel;
        submitData.rotationStart = layer.rotationStart.toISOString();
        submitData.rotationDays = layer.rotationDays;
        submitData.handoffTime = layer.handoffTime;

        await this.layerService.PutScheduleLayer(layer.id!, submitData).toPromise();
    }

    private async saveMembers(layer: EditableLayer): Promise<void> {
        if (!layer.id) return;

        // Delete removed members
        if (layer.originalData) {
            const existingMembers = await layer.originalData.ScheduleLayerMembers;
            for (const existing of existingMembers) {
                const stillExists = layer.members.some(m => m.id === Number(existing.id));
                if (!stillExists) {
                    const submitData = existing.ConvertToSubmitData();
                    submitData.deleted = true;
                    await this.memberService.PutScheduleLayerMember(Number(existing.id), submitData).toPromise();
                }
            }
        }

        // Create/update members
        for (const member of layer.members) {
            if (!member.securityUserObjectGuid) continue;

            if (member.isNew) {
                const newMember = await this.createMember(layer.id, member);
                member.id = Number(newMember.id);
                member.isNew = false;
            } else if (member.id) {
                await this.updateMember(member);
            }
        }
    }

    private async createMember(layerId: number, member: EditableMember): Promise<ScheduleLayerMemberData> {
        const submitData = new ScheduleLayerMemberSubmitData();
        submitData.id = 0;
        submitData.scheduleLayerId = layerId;
        submitData.position = member.position;
        submitData.securityUserObjectGuid = member.securityUserObjectGuid;
        submitData.active = true;
        submitData.deleted = false;
        submitData.versionNumber = 0;

        return this.memberService.PostScheduleLayerMember(submitData).toPromise() as Promise<ScheduleLayerMemberData>;
    }

    private async updateMember(member: EditableMember): Promise<void> {
        if (!member.originalData) return;

        const submitData = member.originalData.ConvertToSubmitData();
        submitData.position = member.position;
        submitData.securityUserObjectGuid = member.securityUserObjectGuid;

        await this.memberService.PutScheduleLayerMember(member.id!, submitData).toPromise();
    }

    // ==================== Override Management ====================

    async loadOverrides(): Promise<void> {
        if (!this.scheduleId) return;

        // Load override types if not already loaded
        if (this.overrideTypes.length === 0) {
            try {
                this.overrideTypes = await this.overrideTypeService.GetScheduleOverrideTypeList().toPromise() || [];
            } catch (error) {
                console.error('Failed to load override types:', error);
            }
        }

        // Load overrides for this schedule
        try {
            const overridesData = await this.overrideService.GetScheduleOverrideList({ onCallScheduleId: this.scheduleId }).toPromise() || [];
            this.overrides = overridesData.map((o: ScheduleOverrideData) => ({
                id: Number(o.id),
                scheduleLayerId: o.scheduleLayerId ? Number(o.scheduleLayerId) : null,
                scheduleOverrideTypeId: Number(o.scheduleOverrideTypeId),
                startDateTime: new Date(o.startDateTime),
                endDateTime: new Date(o.endDateTime),
                originalUserObjectGuid: o.originalUserObjectGuid,
                replacementUserObjectGuid: o.replacementUserObjectGuid,
                reason: o.reason || '',
                isNew: false,
                isModified: false,
                originalData: o
            }));
        } catch (error) {
            console.error('Failed to load overrides:', error);
        }
    }

    getOverrideTypeName(typeId: number): string {
        const overrideType = this.overrideTypes.find(t => Number(t.id) === typeId);
        return overrideType?.name || 'Unknown';
    }

    openAddOverrideModal(): void {
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        tomorrow.setHours(9, 0, 0, 0);

        const endDate = new Date(tomorrow);
        endDate.setDate(endDate.getDate() + 1);

        this.editingOverride = {
            id: null,
            scheduleLayerId: null,
            scheduleOverrideTypeId: this.overrideTypes.find(t => t.name === 'Replace')?.id as number || 1,
            startDateTime: tomorrow,
            endDateTime: endDate,
            originalUserObjectGuid: null,
            replacementUserObjectGuid: null,
            reason: '',
            isNew: true,
            isModified: false,
            originalData: null
        };
        this.showOverrideModal = true;
    }

    openEditOverrideModal(override: EditableOverride): void {
        this.editingOverride = { ...override };
        this.showOverrideModal = true;
    }

    onOverrideStartDateChange(value: string): void {
        if (this.editingOverride && value) {
            this.editingOverride.startDateTime = new Date(value);
        }
    }

    onOverrideEndDateChange(value: string): void {
        if (this.editingOverride && value) {
            this.editingOverride.endDateTime = new Date(value);
        }
    }

    closeOverrideModal(): void {
        this.showOverrideModal = false;
        this.editingOverride = null;
    }

    saveOverrideFromModal(): void {
        if (!this.editingOverride) return;

        if (this.editingOverride.isNew) {
            this.overrides.push({ ...this.editingOverride });
        } else {
            const index = this.overrides.findIndex(o => o.id === this.editingOverride!.id);
            if (index >= 0) {
                this.editingOverride.isModified = true;
                this.overrides[index] = { ...this.editingOverride };
            }
        }

        this.markUnsavedChanges();
        this.closeOverrideModal();
        this.updateTimeline();
    }

    removeOverride(override: EditableOverride): void {
        if (!confirm('Are you sure you want to remove this override?')) return;

        if (!override.isNew && override.id) {
            this.deletedOverrideIds.push(override.id);
        }

        const index = this.overrides.indexOf(override);
        if (index >= 0) {
            this.overrides.splice(index, 1);
        }

        this.markUnsavedChanges();
        this.updateTimeline();
    }

    async saveOverrides(scheduleId: number): Promise<void> {
        // Delete removed overrides
        for (const overrideId of this.deletedOverrideIds) {
            try {
                await this.overrideService.DeleteScheduleOverride(overrideId).toPromise();
            } catch (error) {
                console.error('Failed to delete override:', error);
            }
        }

        // Create/update overrides
        for (const override of this.overrides) {
            try {
                if (override.isNew) {
                    await this.createOverride(scheduleId, override);
                } else if (override.isModified) {
                    await this.updateOverride(override);
                }
            } catch (error) {
                console.error('Failed to save override:', error);
                throw error;
            }
        }

        this.deletedOverrideIds = [];
    }

    private async createOverride(scheduleId: number, override: EditableOverride): Promise<void> {
        const submitData = new ScheduleOverrideSubmitData();
        submitData.onCallScheduleId = scheduleId;
        submitData.scheduleLayerId = override.scheduleLayerId as any;
        submitData.scheduleOverrideTypeId = override.scheduleOverrideTypeId;
        submitData.startDateTime = override.startDateTime.toISOString();
        submitData.endDateTime = override.endDateTime.toISOString();
        submitData.originalUserObjectGuid = override.originalUserObjectGuid;
        submitData.replacementUserObjectGuid = override.replacementUserObjectGuid;
        submitData.reason = override.reason || null;
        submitData.createdByUserObjectGuid = '00000000-0000-0000-0000-000000000000'; // Will be set by server
        submitData.active = true;
        submitData.deleted = false;

        await this.overrideService.PostScheduleOverride(submitData).toPromise();
    }

    private async updateOverride(override: EditableOverride): Promise<void> {
        if (!override.originalData) return;

        const submitData = override.originalData.ConvertToSubmitData();
        submitData.scheduleLayerId = override.scheduleLayerId as any;
        submitData.scheduleOverrideTypeId = override.scheduleOverrideTypeId;
        submitData.startDateTime = override.startDateTime.toISOString();
        submitData.endDateTime = override.endDateTime.toISOString();
        submitData.originalUserObjectGuid = override.originalUserObjectGuid;
        submitData.replacementUserObjectGuid = override.replacementUserObjectGuid;
        submitData.reason = override.reason || null;

        await this.overrideService.PutScheduleOverride(override.id!, submitData).toPromise();
    }

    // ==================== Navigation ====================

    cancel(): void {
        if (this.hasUnsavedChanges) {
            if (!confirm('You have unsaved changes. Are you sure you want to leave?')) {
                return;
            }
        }
        this.router.navigate(['/schedule-management']);
    }

    canDeactivate(): boolean {
        if (this.hasUnsavedChanges) {
            return confirm('You have unsaved changes. Are you sure you want to leave?');
        }
        return true;
    }
}
