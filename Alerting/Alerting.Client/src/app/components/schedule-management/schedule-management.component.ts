import { Component, OnInit, OnDestroy, ViewChild, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, BehaviorSubject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { OnCallScheduleService, OnCallScheduleData, OnCallScheduleSubmitData, OnCallScheduleQueryParameters } from '../../alerting-data-services/on-call-schedule.service';
import { ScheduleLayerService, ScheduleLayerData } from '../../alerting-data-services/schedule-layer.service';
import { AlertService } from '../../services/alert.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';
import { NavigationService } from '../../utility-services/navigation.service';

@Component({
    selector: 'app-schedule-management',
    templateUrl: './schedule-management.component.html',
    styleUrls: ['./schedule-management.component.scss']
})
export class ScheduleManagementComponent implements OnInit, OnDestroy {
    @ViewChild('addEditModal') addEditModalTemplate!: TemplateRef<unknown>;

    // Data
    schedules: OnCallScheduleData[] = [];

    // State
    isLoading = true;
    loadingFilteredCount = false;
    errorMessage: string | null = null;

    // Pagination
    currentPage = 1;
    pageSize = 10;
    totalPages = 1;
    totalCount$ = new BehaviorSubject<number>(0);
    filteredCount$ = new BehaviorSubject<number>(0);

    // Filter
    filterText = '';
    private filterSubject = new Subject<string>();

    // Modal (for quick add)
    private modalRef: NgbModalRef | null = null;
    isAddMode = true;
    isSaving = false;

    // Form fields (for quick add modal)
    formName = '';
    formDescription = '';
    formTimeZoneId = 'UTC';
    formActive = true;

    // Layer counts (schedule id -> layer count)
    layerCounts: Map<number | bigint, number> = new Map();

    // On-call now cache (schedule guid -> user display name)
    onCallNow: Map<string, string> = new Map();

    // Users for name resolution
    private users: AlertingUser[] = [];
    private userLookup: Map<string, string> = new Map();

    // Common timezones for dropdown
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

    private destroy$ = new Subject<void>();

    // For template access
    Math = Math;

    constructor(
        private scheduleService: OnCallScheduleService,
        private scheduleLayerService: ScheduleLayerService,
        private alertService: AlertService,
        private alertingUserService: AlertingUserService,
        private modalService: NgbModal,
        private router: Router,
        private navigationService: NavigationService
    ) { }

    ngOnInit(): void {
        this.loadUsers();
        this.loadSchedules();
        this.setupFilterDebounce();
    }

    private loadUsers(): void {
        this.alertingUserService.getUsers()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (users) => {
                    this.users = users;
                    this.userLookup.clear();
                    users.forEach(u => this.userLookup.set(u.objectGuid, u.displayName));
                }
            });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.modalRef?.close();
    }

    private setupFilterDebounce(): void {
        this.filterSubject
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                takeUntil(this.destroy$)
            )
            .subscribe(filter => {
                this.filterText = filter;
                this.currentPage = 1;
                this.loadSchedules();
                this.loadFilteredCount(filter);
            });
    }

    loadSchedules(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const params: Partial<OnCallScheduleQueryParameters> = {
            active: true,
            deleted: false,
            pageSize: this.pageSize,
            pageNumber: this.currentPage,
            includeRelations: true,
            anyStringContains: this.filterText?.trim() || null
        };

        this.scheduleService.GetOnCallScheduleList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (schedules) => {
                    this.schedules = schedules;
                    this.isLoading = false;
                    this.loadTotalCount();
                    this.loadLayerCounts(schedules);
                    this.loadOnCallNow(schedules);
                },
                error: (err) => {
                    console.error('Error loading schedules:', err);
                    this.errorMessage = 'Failed to load schedules. Please try again.';
                    this.isLoading = false;
                }
            });
    }

    private loadTotalCount(): void {
        this.scheduleService.GetOnCallSchedulesRowCount({ active: true, deleted: false })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    const numCount = Number(count);
                    this.totalCount$.next(numCount);
                    if (!this.filterText) {
                        this.filteredCount$.next(numCount);
                        this.totalPages = Math.ceil(numCount / this.pageSize);
                    }
                },
                error: (err) => console.error('Error loading total count:', err)
            });
    }

    private loadFilteredCount(filter: string): void {
        if (!filter || filter.trim().length === 0) {
            return;
        }

        this.loadingFilteredCount = true;

        this.scheduleService.GetOnCallSchedulesRowCount({
            active: true,
            deleted: false,
            anyStringContains: filter.trim()
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    const numCount = Number(count);
                    this.filteredCount$.next(numCount);
                    this.totalPages = Math.ceil(numCount / this.pageSize);
                    this.loadingFilteredCount = false;
                },
                error: (err) => {
                    console.error('Error loading filtered count:', err);
                    this.loadingFilteredCount = false;
                }
            });
    }

    private loadLayerCounts(schedules: OnCallScheduleData[]): void {
        for (const schedule of schedules) {
            schedule.ScheduleLayers.then(layers => {
                this.layerCounts.set(schedule.id, layers.length);
            }).catch(() => {
                this.layerCounts.set(schedule.id, 0);
            });
        }
    }

    getLayerCount(schedule: OnCallScheduleData): number {
        return this.layerCounts.get(schedule.id) || 0;
    }

    /**
     * Calculate who is currently on-call for each schedule
     */
    private async loadOnCallNow(schedules: OnCallScheduleData[]): Promise<void> {
        for (const schedule of schedules) {
            try {
                const layers = await schedule.ScheduleLayers;
                if (layers.length === 0) {
                    this.onCallNow.set(schedule.objectGuid, 'No layers');
                    continue;
                }

                // Sort by layer level to get Layer 1 (primary) first
                const sortedLayers = layers.sort((a, b) => Number(a.layerLevel) - Number(b.layerLevel));
                const primaryLayer = sortedLayers[0];

                const members = await primaryLayer.ScheduleLayerMembers;
                if (members.length === 0) {
                    this.onCallNow.set(schedule.objectGuid, 'No members');
                    continue;
                }

                // Calculate current on-call based on rotation
                const rotationMs = Number(primaryLayer.rotationDays) * 24 * 60 * 60 * 1000;
                const rotationStart = new Date(primaryLayer.rotationStart);

                // Parse handoff time
                const handoffParts = (primaryLayer.handoffTime || '09:00').split(':');
                const handoffHour = parseInt(handoffParts[0], 10);
                const handoffMinute = parseInt(handoffParts[1] || '0', 10);
                rotationStart.setHours(handoffHour, handoffMinute, 0, 0);

                const now = new Date();
                const elapsedMs = now.getTime() - rotationStart.getTime();
                const rotationIndex = Math.floor(elapsedMs / rotationMs);

                // Sort members by position
                const sortedMembers = members.sort((a, b) => Number(a.position) - Number(b.position));
                const memberIndex = ((rotationIndex % sortedMembers.length) + sortedMembers.length) % sortedMembers.length;
                const currentMember = sortedMembers[memberIndex];

                // Resolve user name
                const userName = this.userLookup.get(currentMember.securityUserObjectGuid) || 'Unknown';
                this.onCallNow.set(schedule.objectGuid, userName);
            } catch (err) {
                console.error('Error calculating on-call for schedule:', schedule.name, err);
                this.onCallNow.set(schedule.objectGuid, '—');
            }
        }
    }

    getOnCallNow(schedule: OnCallScheduleData): string {
        return this.onCallNow.get(schedule.objectGuid) || 'Loading...';
    }

    // Filter handlers
    onFilterChange(value: string): void {
        this.filterSubject.next(value);
    }

    clearFilter(): void {
        this.filterText = '';
        this.filterSubject.next('');
    }

    // Pagination
    onPageChange(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
            this.loadSchedules();
        }
    }

    // Modal handlers
    openAddModal(): void {
        this.isAddMode = true;
        this.formName = '';
        this.formDescription = '';
        this.formTimeZoneId = 'UTC';
        this.formActive = true;

        this.modalRef = this.modalService.open(this.addEditModalTemplate, {
            centered: true,
            backdrop: 'static'
        });
    }

    closeModal(): void {
        this.modalRef?.close();
        this.modalRef = null;
    }

    saveSchedule(): void {
        if (!this.formName.trim()) {
            this.alertService.showErrorMessage('Validation Error', 'Schedule name is required.');
            return;
        }

        this.isSaving = true;

        const submitData = new OnCallScheduleSubmitData();
        submitData.id = 0;
        submitData.name = this.formName.trim();
        submitData.description = this.formDescription.trim() || null;
        submitData.timeZoneId = this.formTimeZoneId;
        submitData.active = this.formActive;
        submitData.deleted = false;
        submitData.versionNumber = 0;

        this.scheduleService.PostOnCallSchedule(submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (created) => {
                    this.isSaving = false;
                    this.closeModal();
                    this.scheduleService.ClearAllCaches();
                    this.alertService.showSuccessMessage('Schedule Created', `"${created.name}" has been created.`);
                    // Navigate to editor for the new schedule
                    this.router.navigate(['/schedule-management', created.id, 'edit']);
                },
                error: (err) => {
                    console.error('Error creating schedule:', err);
                    this.isSaving = false;
                    this.alertService.showHttpErrorMessage('Create Failed', err);
                }
            });
    }

    // Navigation to editor
    editSchedule(schedule: OnCallScheduleData): void {
        this.router.navigate(['/schedule-management', schedule.id, 'edit']);
    }

    // Delete
    deleteSchedule(schedule: OnCallScheduleData): void {
        if (!confirm(`Are you sure you want to delete "${schedule.name}"? This action cannot be undone.`)) {
            return;
        }

        const submitData = schedule.ConvertToSubmitData();
        submitData.deleted = true;

        this.scheduleService.PutOnCallSchedule(Number(schedule.id), submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.scheduleService.ClearAllCaches();
                    this.alertService.showSuccessMessage('Schedule Deleted', `"${schedule.name}" has been deleted.`);
                    this.loadSchedules();
                },
                error: (err) => {
                    console.error('Error deleting schedule:', err);
                    this.alertService.showHttpErrorMessage('Delete Failed', err);
                }
            });
    }

    // Status helpers
    getStatusBadgeClass(schedule: OnCallScheduleData): string {
        if (schedule.deleted) return 'badge-deleted';
        if (!schedule.active) return 'badge-inactive';
        return 'badge-active';
    }

    getStatusText(schedule: OnCallScheduleData): string {
        if (schedule.deleted) return 'Deleted';
        if (!schedule.active) return 'Inactive';
        return 'Active';
    }

    trackById(index: number, schedule: OnCallScheduleData): number | bigint {
        return schedule.id;
    }

    getTimezoneName(tzId: string): string {
        const tz = this.commonTimezones.find(t => t.id === tzId);
        return tz ? tz.label : tzId;
    }

    // Navigation
    goBack(): void {
        this.navigationService.goBack();
    }

    canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }
}
