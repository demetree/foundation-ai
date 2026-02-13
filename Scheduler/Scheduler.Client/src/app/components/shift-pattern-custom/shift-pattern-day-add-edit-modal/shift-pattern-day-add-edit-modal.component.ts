/**
 * ShiftPatternDayAddEditModalComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Modal for creating / editing individual ShiftPatternDay records within a pattern.
 * Uses NgbActiveModal (opened programmatically via NgbModal.open()).
 */
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ShiftPatternDayService, ShiftPatternDayData, ShiftPatternDaySubmitData } from '../../../scheduler-data-services/shift-pattern-day.service';

@Component({
    selector: 'app-shift-pattern-day-add-edit-modal',
    templateUrl: './shift-pattern-day-add-edit-modal.component.html',
    styleUrls: ['./shift-pattern-day-add-edit-modal.component.scss']
})
export class ShiftPatternDayAddEditModalComponent implements OnInit {

    // Set by the caller before modal opens
    shiftPatternId!: number | bigint;
    existingDay: ShiftPatternDayData | null = null;

    dayForm!: FormGroup;
    isSaving = false;
    isEditMode = false;

    daysOfWeek = [
        { value: 1, label: 'Sunday' },
        { value: 2, label: 'Monday' },
        { value: 3, label: 'Tuesday' },
        { value: 4, label: 'Wednesday' },
        { value: 5, label: 'Thursday' },
        { value: 6, label: 'Friday' },
        { value: 7, label: 'Saturday' }
    ];

    constructor(
        public activeModal: NgbActiveModal,
        private dayService: ShiftPatternDayService,
        private alertService: AlertService,
        private fb: FormBuilder
    ) { }

    ngOnInit(): void {
        this.isEditMode = !!this.existingDay;

        this.dayForm = this.fb.group({
            dayOfWeek: [this.existingDay?.dayOfWeek || 2, Validators.required],
            startTime: [this.formatTimeForInput(this.existingDay?.startTime) || '07:00', Validators.required],
            hours: [this.existingDay?.hours || 8, [Validators.required, Validators.min(0.25), Validators.max(24)]],
            label: [this.existingDay?.label || '']
        });
    }

    submitForm(): void {
        if (this.isSaving || !this.dayForm.valid) {
            this.dayForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const f = this.dayForm.getRawValue();

        const data: ShiftPatternDaySubmitData = {
            id: this.existingDay?.id || 0,
            shiftPatternId: this.shiftPatternId as number,
            dayOfWeek: Number(f.dayOfWeek),
            startTime: this.formatTimeForServer(f.startTime),
            hours: Number(f.hours),
            label: f.label?.trim() || null,
            versionNumber: this.existingDay?.versionNumber || 0,
            active: this.existingDay?.active ?? true,
            deleted: this.existingDay?.deleted ?? false
        };

        const save$ = this.isEditMode
            ? this.dayService.PutShiftPatternDay(data.id, data)
            : this.dayService.PostShiftPatternDay(data);

        save$.pipe(finalize(() => this.isSaving = false)).subscribe({
            next: (saved) => {
                this.dayService.ClearAllCaches();
                this.alertService.showMessage(
                    `Day ${this.getDayLabel(saved.dayOfWeek as number)} ${this.isEditMode ? 'updated' : 'added'}`,
                    '', MessageSeverity.success);
                this.activeModal.close(saved);
            },
            error: (err) => {
                this.alertService.showMessage('Error saving day', err?.message || '', MessageSeverity.error);
            }
        });
    }

    cancel(): void {
        this.activeModal.dismiss('cancel');
    }

    // ── Helpers ──

    getDayLabel(val: number): string {
        return this.daysOfWeek.find(d => d.value === val)?.label || '?';
    }

    private formatTimeForInput(isoTime: string | null | undefined): string {
        if (!isoTime) return '07:00';
        // ISO time might be "07:00:00" or "1970-01-01T07:00:00Z"
        const match = isoTime.match(/(\d{2}:\d{2})/);
        return match ? match[1] : '07:00';
    }

    private formatTimeForServer(timeStr: string): string {
        // Server expects ISO 8601 time string like "07:00:00"
        if (!timeStr) return '07:00:00';
        const parts = timeStr.split(':');
        return `${parts[0].padStart(2, '0')}:${(parts[1] || '00').padStart(2, '0')}:00`;
    }
}
