/**
 * ResourceShiftTabComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Shifts / Work Schedule tab for the Resource detail page.
 *
 * Displays a visual weekly timetable grid (Mon–Sun) with colored shift blocks,
 * plus a list view with edit/delete actions below.
 *
 * Data loaded imperatively when tab becomes active via resource.ResourceShifts.
 */
import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild, TemplateRef } from '@angular/core';
import { Subject, forkJoin } from 'rxjs';
import { ResourceData, ResourceService, ResourceSubmitData } from '../../../scheduler-data-services/resource.service';
import { ResourceShiftService, ResourceShiftData, ResourceShiftSubmitData } from '../../../scheduler-data-services/resource-shift.service';
import { ShiftPatternService, ShiftPatternData } from '../../../scheduler-data-services/shift-pattern.service';
import { ShiftPatternDayService, ShiftPatternDayData } from '../../../scheduler-data-services/shift-pattern-day.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ResourceShiftAddEditModalComponent } from '../resource-shift-add-edit-modal/resource-shift-add-edit-modal.component';
import { ConfirmationService } from '../../../services/confirmation-service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-resource-shift-tab',
    templateUrl: './resource-shift-tab.component.html',
    styleUrls: ['./resource-shift-tab.component.scss']
})
export class ResourceShiftTabComponent implements OnChanges {

    @ViewChild('applyPatternModal') applyPatternModal!: TemplateRef<any>;

    @Input() resource!: ResourceData | null;

    @Output() resourceShiftChanged = new Subject<ResourceShiftData>();

    public shifts: ResourceShiftData[] | null = null;
    public isLoading = true;
    public error: string | null = null;

    // Apply Pattern state
    public shiftPatterns: ShiftPatternData[] = [];
    public selectedPatternId: number | null = null;
    public applyMode: 'replace' | 'merge' = 'replace';
    public isApplyingPattern = false;
    public isPatternsLoading = false;

    // Days ordered Monday-first for the timetable
    public readonly dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    public readonly dayAbbrevs = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    public readonly timetableDayOrder = [1, 2, 3, 4, 5, 6, 0]; // Mon–Sun

    // Timetable time range (6 AM – 10 PM)
    public readonly timetableStartHour = 6;
    public readonly timetableEndHour = 22;
    public readonly timetableTotalHours = 16;

    // Day colors for timetable blocks
    public readonly dayColors: { [key: number]: string } = {
        0: '#94a3b8', // Sunday - slate
        1: '#6366f1', // Monday - indigo
        2: '#8b5cf6', // Tuesday - violet
        3: '#06b6d4', // Wednesday - cyan
        4: '#f59e0b', // Thursday - amber
        5: '#14b8a6', // Friday - teal
        6: '#f472b6'  // Saturday - pink
    };

    /** Template helper — coerces bigint/number to number for indexing. */
    public toNum(val: any): number {
        return Number(val);
    }

    constructor(
        private modalService: NgbModal,
        private shiftService: ResourceShiftService,
        private patternService: ShiftPatternService,
        private patternDayService: ShiftPatternDayService,
        private resourceService: ResourceService,
        private confirmationService: ConfirmationService,
        private alertService: AlertService
    ) { }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['resource'] && this.resource) {
            this.resource.ClearResourceShiftsCache();
            this.loadShifts();
        }
    }


    // =========================================================================
    // Data Loading
    // =========================================================================

    public loadShifts(): void {
        if (!this.resource) {
            this.shifts = [];
            this.isLoading = false;
            return;
        }

        this.isLoading = true;
        this.error = null;

        this.resource.ResourceShifts
            .then(shifts => {
                // Sort by dayOfWeek (Mon-first), then by startTime
                this.shifts = shifts.sort((a, b) => {
                    const dayA = this.timetableDayOrder.indexOf(Number(a.dayOfWeek));
                    const dayB = this.timetableDayOrder.indexOf(Number(b.dayOfWeek));
                    if (dayA !== dayB) return dayA - dayB;
                    return (a.startTime || '').localeCompare(b.startTime || '');
                });
                this.isLoading = false;
            })
            .catch(err => {
                console.error('Failed to load resource shifts', err);
                this.error = 'Unable to load shift schedule';
                this.shifts = [];
                this.isLoading = false;
            });
    }


    // =========================================================================
    // Timetable Helpers
    // =========================================================================

    /**
     * Get shifts for a specific day of week (for timetable rows).
     */
    public getShiftsForDay(dayOfWeek: number): ResourceShiftData[] {
        if (!this.shifts) return [];
        return this.shifts.filter(s => Number(s.dayOfWeek) === dayOfWeek);
    }

    /**
     * Calculate the left position (%) of a shift block in the timetable.
     */
    public getShiftLeft(shift: ResourceShiftData): number {
        const startHour = this.parseStartTimeHours(shift.startTime);
        const offsetFromStart = startHour - this.timetableStartHour;
        return Math.max(0, (offsetFromStart / this.timetableTotalHours) * 100);
    }

    /**
     * Calculate the width (%) of a shift block in the timetable.
     */
    public getShiftWidth(shift: ResourceShiftData): number {
        const hours = Number(shift.hours) || 0;
        const width = (hours / this.timetableTotalHours) * 100;
        // Clamp to not overflow
        const left = this.getShiftLeft(shift);
        return Math.min(width, 100 - left);
    }

    /**
     * Parse a time string like "09:00" or "09:00:00" to decimal hours (e.g., 9.5 for 9:30).
     */
    private parseStartTimeHours(timeStr: string | null | undefined): number {
        if (!timeStr) return 0;
        const parts = String(timeStr).split(':');
        return (Number(parts[0]) || 0) + (Number(parts[1]) || 0) / 60;
    }

    /**
     * Format a time string for display (e.g., "09:00" → "9:00 AM").
     */
    public formatTime(timeStr: string | null | undefined): string {
        if (!timeStr) return '';
        const parts = String(timeStr).split(':');
        let hours = Number(parts[0]) || 0;
        const minutes = String(parts[1] || '00').padStart(2, '0');
        const ampm = hours >= 12 ? 'PM' : 'AM';
        if (hours === 0) hours = 12;
        else if (hours > 12) hours -= 12;
        return `${hours}:${minutes} ${ampm}`;
    }

    /**
     * Calculate end time from startTime + hours for display.
     */
    public formatEndTime(shift: ResourceShiftData): string {
        if (!shift.startTime || shift.hours == null) return '';
        const startHours = this.parseStartTimeHours(shift.startTime);
        const endDecimal = startHours + Number(shift.hours);
        const endH = Math.floor(endDecimal);
        const endM = Math.round((endDecimal % 1) * 60);
        let display = endH;
        const ampm = display >= 12 ? 'PM' : 'AM';
        if (display === 0) display = 12;
        else if (display > 12) display -= 12;
        return `${display}:${String(endM).padStart(2, '0')} ${ampm}`;
    }

    /**
     * Format hours for display (e.g., 8 → "8h", 8.5 → "8.5h").
     */
    public formatHours(hours: number | null | undefined): string {
        if (hours == null) return '';
        const h = Number(hours);
        return h % 1 === 0 ? `${h}h` : `${h}h`;
    }

    /**
     * Get hour labels for the timetable header.
     */
    public get timetableHourLabels(): number[] {
        const labels: number[] = [];
        for (let h = this.timetableStartHour; h <= this.timetableEndHour; h++) {
            labels.push(h);
        }
        return labels;
    }

    /**
     * Format hour number for timetable header (e.g., 13 → "1 PM").
     */
    public formatHourLabel(hour: number): string {
        if (hour === 0 || hour === 24) return '12 AM';
        if (hour === 12) return '12 PM';
        if (hour < 12) return `${hour} AM`;
        return `${hour - 12} PM`;
    }

    /**
     * Check if the timetable has any shifts to display.
     */
    public get hasTimetableData(): boolean {
        return this.shifts != null && this.shifts.length > 0;
    }


    // =========================================================================
    // CRUD Actions
    // =========================================================================

    public openAddShiftModal(): void {
        if (!this.resource) return;

        const modalRef = this.modalService.open(ResourceShiftAddEditModalComponent, {
            size: 'md',
            backdrop: 'static'
        });

        modalRef.componentInstance.resourceId = this.resource.id;
        modalRef.componentInstance.resourceName = this.resource.name;
        modalRef.componentInstance.timeZoneId = this.resource.timeZoneId;

        modalRef.result.then(
            (data) => {
                this.resource?.ClearResourceShiftsCache();
                this.resourceShiftChanged.next(data);
                this.loadShifts();
            },
            () => { }
        );
    }

    public openEditShiftModal(shift: ResourceShiftData): void {
        if (!this.resource) return;

        const modalRef = this.modalService.open(ResourceShiftAddEditModalComponent, {
            size: 'md',
            backdrop: 'static'
        });

        modalRef.componentInstance.resourceId = this.resource.id;
        modalRef.componentInstance.resourceName = this.resource.name;
        modalRef.componentInstance.timeZoneId = this.resource.timeZoneId;
        modalRef.componentInstance.existingShift = shift;

        modalRef.result.then(
            (data) => {
                this.resource?.ClearResourceShiftsCache();
                this.resourceShiftChanged.next(data);
                this.loadShifts();
            },
            () => { }
        );
    }

    public deleteShift(shift: ResourceShiftData): void {
        if (!confirm(`Delete this ${this.dayNames[Number(shift.dayOfWeek)]} shift (${this.formatTime(shift.startTime)} – ${this.formatEndTime(shift)})?`)) {
            return;
        }

        const submitData: ResourceShiftSubmitData = {
            id: shift.id,
            resourceId: shift.resourceId,
            dayOfWeek: shift.dayOfWeek,
            timeZoneId: shift.timeZoneId,
            startTime: shift.startTime,
            hours: Number(shift.hours),
            label: shift.label,
            versionNumber: shift.versionNumber,
            active: false,
            deleted: true
        };

        this.shiftService.PutResourceShift(Number(shift.id), submitData).subscribe({
            next: () => {
                this.shiftService.ClearAllCaches();
                this.alertService.showMessage('Shift deleted', '', MessageSeverity.success);
                this.resource?.ClearResourceShiftsCache();
                this.resourceShiftChanged.next(shift);
                this.loadShifts();
            },
            error: (err) => {
                this.alertService.showMessage('Failed to delete shift', err.message || 'Unknown error', MessageSeverity.error);
            }
        });
    }


    // =========================================================================
    // Apply Shift Pattern
    // =========================================================================

    /**
     * Opens the Apply Shift Pattern modal.
     */
    public openApplyPatternModal(): void {
        if (!this.resource) return;

        this.selectedPatternId = null;
        this.applyMode = 'replace';
        this.isPatternsLoading = true;

        this.patternService.GetShiftPatternList({ active: true })
            .subscribe({
                next: (patterns) => {
                    this.shiftPatterns = patterns;
                    this.isPatternsLoading = false;
                },
                error: () => { this.isPatternsLoading = false; }
            });

        this.modalService.open(this.applyPatternModal, {
            size: 'md',
            backdrop: 'static'
        });
    }

    /**
     * Get the currently selected pattern.
     */
    public get selectedPattern(): ShiftPatternData | null {
        if (!this.selectedPatternId) return null;
        return this.shiftPatterns.find(p => Number(p.id) === this.selectedPatternId) || null;
    }

    /**
     * Apply the selected shift pattern to this resource.
     * - In 'replace' mode: deletes existing shifts first, then creates new ones.
     * - In 'merge' mode: creates new shifts without deleting existing ones.
     */
    public applyPattern(modal: any): void {
        if (!this.resource || !this.selectedPatternId || this.isApplyingPattern) return;

        this.isApplyingPattern = true;
        const patternId = this.selectedPatternId;

        // 1. Fetch the pattern days
        this.patternService.GetShiftPatternDaysForShiftPattern(patternId)
            .subscribe({
                next: (patternDays) => {
                    if (patternDays.length === 0) {
                        this.alertService.showMessage('Pattern has no days defined', '', MessageSeverity.warn);
                        this.isApplyingPattern = false;
                        return;
                    }

                    if (this.applyMode === 'replace' && this.shifts && this.shifts.length > 0) {
                        // Soft-delete existing shifts first
                        const deleteOps = this.shifts.map(s => {
                            const submitData: ResourceShiftSubmitData = {
                                id: s.id,
                                resourceId: s.resourceId,
                                dayOfWeek: s.dayOfWeek,
                                timeZoneId: s.timeZoneId,
                                startTime: s.startTime,
                                hours: Number(s.hours),
                                label: s.label,
                                versionNumber: s.versionNumber,
                                active: false,
                                deleted: true
                            };
                            return this.shiftService.PutResourceShift(Number(s.id), submitData);
                        });

                        forkJoin(deleteOps).subscribe({
                            next: () => this.createShiftsFromPattern(patternDays, patternId, modal),
                            error: (err) => {
                                this.alertService.showMessage('Error removing existing shifts', err?.message || '', MessageSeverity.error);
                                this.isApplyingPattern = false;
                            }
                        });
                    } else {
                        // Merge mode or no existing shifts
                        this.createShiftsFromPattern(patternDays, patternId, modal);
                    }
                },
                error: (err) => {
                    this.alertService.showMessage('Error loading pattern days', err?.message || '', MessageSeverity.error);
                    this.isApplyingPattern = false;
                }
            });
    }

    /**
     * Bulk-create ResourceShift records from pattern days and update the resource's shiftPatternId.
     */
    private createShiftsFromPattern(days: ShiftPatternDayData[], patternId: number, modal: any): void {
        if (!this.resource) return;

        const createOps = days.map(d => {
            const submitData: ResourceShiftSubmitData = {
                id: 0,
                resourceId: this.resource!.id,
                dayOfWeek: d.dayOfWeek,
                timeZoneId: this.resource!.timeZoneId,
                startTime: d.startTime,
                hours: Number(d.hours),
                label: d.label,
                versionNumber: 0,
                active: true,
                deleted: false
            };
            return this.shiftService.PostResourceShift(submitData);
        });

        forkJoin(createOps).subscribe({
            next: () => {
                // Update the resource's shiftPatternId
                this.updateResourceShiftPatternId(patternId, modal);
            },
            error: (err) => {
                this.alertService.showMessage('Error creating shifts', err?.message || '', MessageSeverity.error);
                this.isApplyingPattern = false;
            }
        });
    }

    /**
     * Update the resource record to set the shiftPatternId FK.
     */
    private updateResourceShiftPatternId(patternId: number, modal: any): void {
        if (!this.resource) return;

        const submitData: ResourceSubmitData = this.resourceService.ConvertToResourceSubmitData(this.resource);
        submitData.shiftPatternId = patternId;

        this.resourceService.PutResource(Number(this.resource.id), submitData).subscribe({
            next: () => {
                this.resourceService.ClearAllCaches();
                this.shiftService.ClearAllCaches();
                this.resource?.ClearResourceShiftsCache();
                this.alertService.showMessage(
                    `Shift pattern applied successfully (${this.applyMode === 'replace' ? 'replaced' : 'merged'})`,
                    '', MessageSeverity.success);
                this.isApplyingPattern = false;
                modal.close();
                this.loadShifts();
            },
            error: (err) => {
                this.alertService.showMessage('Shifts created but failed to update resource', err?.message || '', MessageSeverity.warn);
                this.isApplyingPattern = false;
                modal.close();
                this.loadShifts();
            }
        });
    }
}
