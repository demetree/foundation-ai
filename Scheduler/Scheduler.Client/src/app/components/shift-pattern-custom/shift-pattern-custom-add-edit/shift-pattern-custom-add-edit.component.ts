/**
 * ShiftPatternCustomAddEditComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Modal form for creating / editing ShiftPattern records.
 * Follows the same pattern as ResourceCustomAddEditComponent.
 */
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ShiftPatternService, ShiftPatternData, ShiftPatternSubmitData } from '../../../scheduler-data-services/shift-pattern.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { AuthService } from '../../../services/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';

@Component({
    selector: 'app-shift-pattern-custom-add-edit',
    templateUrl: './shift-pattern-custom-add-edit.component.html',
    styleUrls: ['./shift-pattern-custom-add-edit.component.scss']
})
export class ShiftPatternCustomAddEditComponent {

    @ViewChild('patternModal') patternModal!: TemplateRef<any>;
    @Output() patternChanged = new Subject<ShiftPatternData[]>();
    @Input() showAddButton = true;
    @Input() navigateToDetailsAfterAdd = true;

    public patternData: ShiftPatternData | null = null;
    public submitData: ShiftPatternSubmitData | null = null;

    patternForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        timeZoneId: [this.currentUserService.defaultTimeZoneId, Validators.required],
        color: ['#6366f1'],
        versionNumber: [''],
        active: [true],
        deleted: [false]
    });

    private modalRef: NgbModalRef | undefined;
    public isEditMode = false;
    public objectGuid = '';
    public modalIsDisplayed = false;
    public isSaving = false;

    public timeZones$ = this.timeZoneService.GetTimeZoneList();

    constructor(
        private modalService: NgbModal,
        private patternService: ShiftPatternService,
        private timeZoneService: TimeZoneService,
        private authService: AuthService,
        private alertService: AlertService,
        private currentUserService: CurrentUserService,
        private router: Router,
        private fb: FormBuilder
    ) { }

    // ── Open ──────────────────────────────────────────────

    openModal(existing?: ShiftPatternData): void {

        if (existing) {
            if (!this.patternService.userIsSchedulerShiftPatternReader()) {
                this.alertService.showMessage(
                    `${this.authService.currentUser?.userName} does not have permission to read Shift Patterns`,
                    '', MessageSeverity.info);
                return;
            }
            this.patternData = existing;
            this.submitData = this.patternService.ConvertToShiftPatternSubmitData(existing);
            this.isEditMode = true;
            this.objectGuid = existing.objectGuid;
            this.buildFormValues(existing);
        } else {
            if (!this.patternService.userIsSchedulerShiftPatternWriter()) {
                this.alertService.showMessage(
                    `${this.authService.currentUser?.userName} does not have permission to manage Shift Patterns`,
                    '', MessageSeverity.info);
                return;
            }
            this.patternData = null;
            this.isEditMode = false;
            this.buildFormValues(null);
        }

        this.modalRef = this.modalService.open(this.patternModal, {
            size: 'md',
            scrollable: true,
            backdrop: 'static',
            keyboard: true,
            windowClass: 'custom-modal'
        });
        this.modalIsDisplayed = true;
    }

    // ── Close ─────────────────────────────────────────────

    closeModal(): void {
        this.modalRef?.dismiss('cancel');
        this.modalIsDisplayed = false;
    }

    // ── Submit ────────────────────────────────────────────

    submitForm(): void {
        if (this.isSaving) return;

        if (!this.patternService.userIsSchedulerShiftPatternWriter()) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to manage Shift Patterns`,
                '', MessageSeverity.info);
            return;
        }

        if (!this.patternForm.valid) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.patternForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const f = this.patternForm.getRawValue();

        const data: ShiftPatternSubmitData = {
            id: this.submitData?.id || 0,
            name: f.name!.trim(),
            description: f.description?.trim() || null,
            timeZoneId: Number(f.timeZoneId),
            color: f.color?.trim() || null,
            versionNumber: this.submitData?.versionNumber || 0,
            active: f.active ?? true,
            deleted: f.deleted ?? false
        };

        const save$ = this.isEditMode
            ? this.patternService.PutShiftPattern(data.id, data)
            : this.patternService.PostShiftPattern(data);

        save$.pipe(finalize(() => this.isSaving = false)).subscribe({
            next: (saved) => {
                this.patternService.ClearAllCaches();
                this.patternChanged.next([saved]);
                this.alertService.showMessage(
                    `Shift Pattern "${saved.name}" ${this.isEditMode ? 'updated' : 'created'} successfully`,
                    '', MessageSeverity.success);
                this.modalRef?.close(saved);
                this.modalIsDisplayed = false;

                if (!this.isEditMode && this.navigateToDetailsAfterAdd) {
                    this.router.navigate(['/shiftpattern', saved.id]);
                }
            },
            error: (err) => {
                this.alertService.showMessage('Error saving Shift Pattern', err?.message || '', MessageSeverity.error);
            }
        });
    }

    // ── Helpers ───────────────────────────────────────────

    private buildFormValues(p: ShiftPatternData | null): void {
        this.patternForm.reset({
            name: p?.name || '',
            description: p?.description || '',
            timeZoneId: p?.timeZoneId || this.currentUserService.defaultTimeZoneId,
            color: p?.color || '#6366f1',
            versionNumber: p?.versionNumber || '',
            active: p?.active ?? true,
            deleted: p?.deleted ?? false
        });
    }

    public userIsWriter(): boolean {
        return this.patternService.userIsSchedulerShiftPatternWriter();
    }
}
