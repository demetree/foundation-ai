/**
 * ResourceShiftAddEditModalComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Modal for adding or editing a resource shift.
 *
 * Supports both creation (POST) and update (PUT) modes.
 * When existingShift is provided, pre-fills the form for editing.
 */
import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ResourceShiftService, ResourceShiftSubmitData, ResourceShiftData } from '../../../scheduler-data-services/resource-shift.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-resource-shift-add-edit-modal',
    templateUrl: './resource-shift-add-edit-modal.component.html',
    styleUrls: ['./resource-shift-add-edit-modal.component.scss']
})
export class ResourceShiftAddEditModalComponent implements OnInit {

    @Input() resourceId!: number;
    @Input() resourceName?: string;
    @Input() timeZoneId!: number;

    /**
     * If provided, the modal operates in Edit mode.
     */
    @Input() existingShift?: ResourceShiftData;

    public shiftForm: FormGroup;
    public isSaving = false;

    public readonly dayOptions = [
        { value: 1, label: 'Monday' },
        { value: 2, label: 'Tuesday' },
        { value: 3, label: 'Wednesday' },
        { value: 4, label: 'Thursday' },
        { value: 5, label: 'Friday' },
        { value: 6, label: 'Saturday' },
        { value: 0, label: 'Sunday' }
    ];

    public get isEditMode(): boolean {
        return this.existingShift != null;
    }

    constructor(
        public activeModal: NgbActiveModal,
        private fb: FormBuilder,
        private shiftService: ResourceShiftService,
        private alertService: AlertService
    ) {
        this.shiftForm = this.fb.group({
            dayOfWeek: [1, Validators.required],
            startTime: ['09:00', Validators.required],
            hours: [8, [Validators.required, Validators.min(0.5), Validators.max(24)]],
            label: ['']
        });
    }

    ngOnInit(): void {
        if (this.existingShift) {
            // Pre-fill form for editing
            const timeStr = this.existingShift.startTime
                ? String(this.existingShift.startTime).substring(0, 5) // "HH:mm"
                : '09:00';

            this.shiftForm.patchValue({
                dayOfWeek: Number(this.existingShift.dayOfWeek),
                startTime: timeStr,
                hours: Number(this.existingShift.hours),
                label: this.existingShift.label || ''
            });
        }
    }

    public submit(): void {
        if (this.isSaving || !this.shiftForm.valid) {
            return;
        }

        this.isSaving = true;
        const formValue = this.shiftForm.value;

        // Build the startTime as "HH:mm:00" for the server
        const startTime = formValue.startTime + ':00';

        if (this.isEditMode) {
            // Update existing shift
            const submitData: ResourceShiftSubmitData = {
                id: this.existingShift!.id,
                resourceId: this.resourceId,
                dayOfWeek: formValue.dayOfWeek,
                timeZoneId: this.timeZoneId,
                startTime: startTime,
                hours: formValue.hours,
                label: formValue.label?.trim() || null,
                versionNumber: this.existingShift!.versionNumber,
                active: true,
                deleted: false
            };

            this.shiftService.PutResourceShift(Number(this.existingShift!.id), submitData).subscribe({
                next: (updated) => {
                    this.shiftService.ClearAllCaches();
                    this.alertService.showMessage('Shift updated successfully', '', MessageSeverity.success);
                    this.activeModal.close(updated);
                },
                error: (err) => {
                    this.alertService.showMessage('Failed to update shift', err.message || 'Unknown error', MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        } else {
            // Create new shift
            const submitData: ResourceShiftSubmitData = {
                id: 0 as any,
                resourceId: this.resourceId,
                dayOfWeek: formValue.dayOfWeek,
                timeZoneId: this.timeZoneId,
                startTime: startTime,
                hours: formValue.hours,
                label: formValue.label?.trim() || null,
                versionNumber: 1,
                active: true,
                deleted: false
            };

            this.shiftService.PostResourceShift(submitData).subscribe({
                next: (newShift) => {
                    this.shiftService.ClearAllCaches();
                    this.alertService.showMessage('Shift added successfully', '', MessageSeverity.success);
                    this.activeModal.close(newShift);
                },
                error: (err) => {
                    this.alertService.showMessage('Failed to add shift', err.message || 'Unknown error', MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        }
    }

    public cancel(): void {
        this.activeModal.dismiss('cancel');
    }
}
