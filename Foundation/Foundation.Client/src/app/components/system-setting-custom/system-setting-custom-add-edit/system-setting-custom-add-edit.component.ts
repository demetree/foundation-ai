//
// System Setting Custom Add/Edit Component
//
// Modal-based add/edit form for System Settings.
// Follows the user-custom-add-edit pattern with openModal() method.
//

import { Component, ViewChild, Output, Input, TemplateRef, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SystemSettingService, SystemSettingData, SystemSettingSubmitData } from '../../../security-data-services/system-setting.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-system-setting-custom-add-edit',
    templateUrl: './system-setting-custom-add-edit.component.html',
    styleUrls: ['./system-setting-custom-add-edit.component.scss']
})
export class SystemSettingCustomAddEditComponent implements OnInit, OnDestroy {

    //
    // Modal template reference
    //
    @ViewChild('settingModal') settingModal!: TemplateRef<any>;

    //
    // Output when setting is changed (created or updated)
    //
    @Output() settingChanged = new Subject<SystemSettingData>();

    //
    // Input for pre-seeded data
    //
    @Input() preSeededData: Partial<SystemSettingData> | null = null;

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();

    //
    // Form and modal state
    //
    public settingForm!: FormGroup;
    private modalRef: NgbModalRef | undefined;
    public isEditMode: boolean = false;
    public modalIsDisplayed: boolean = false;
    public isSaving: boolean = false;

    //
    // Current setting being edited
    //
    private currentSettingId: number | null = null;


    constructor(
        private fb: FormBuilder,
        private modalService: NgbModal,
        private router: Router,
        private authService: AuthService,
        private alertService: AlertService,
        private systemSettingService: SystemSettingService
    ) {
        this.initForm();
    }


    ngOnInit(): void {
        // Initialize form
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Form initialization
    //
    private initForm(): void {
        this.settingForm = this.fb.group({
            name: ['', [Validators.required, Validators.maxLength(256)]],
            description: ['', [Validators.maxLength(1024)]],
            value: ['', [Validators.maxLength(4096)]]
        });
    }


    //
    // Open modal for add or edit
    //
    public openModal(settingData?: SystemSettingData): void {

        //
        // Check permissions
        //
        if (!this.systemSettingService.userIsSecuritySystemSettingWriter()) {
            this.alertService.showMessage(
                'Permission Denied',
                'You do not have permission to modify system settings.',
                MessageSeverity.warn
            );
            return;
        }

        //
        // Set mode and populate form
        //
        if (settingData != null) {
            this.isEditMode = true;
            this.currentSettingId = Number(settingData.id);
            this.populateForm(settingData);
        } else {
            this.isEditMode = false;
            this.currentSettingId = null;
            this.resetForm();

            // Apply pre-seeded data if provided
            if (this.preSeededData != null) {
                this.settingForm.patchValue(this.preSeededData);
            }
        }

        //
        // Open modal
        //
        this.modalRef = this.modalService.open(this.settingModal, {
            size: 'lg',
            scrollable: true,
            backdrop: 'static',
            keyboard: true,
            windowClass: 'system-setting-modal'
        });

        this.modalIsDisplayed = true;

        this.modalRef.result.then(
            () => { this.modalIsDisplayed = false; },
            () => { this.modalIsDisplayed = false; }
        );
    }


    //
    // Close modal
    //
    public closeModal(): void {
        if (this.modalRef) {
            this.modalRef.dismiss('cancel');
        }
        this.modalIsDisplayed = false;
    }


    //
    // Populate form with existing data
    //
    private populateForm(setting: SystemSettingData): void {
        this.settingForm.patchValue({
            name: setting.name ?? '',
            description: setting.description ?? '',
            value: setting.value ?? ''
        });
        this.settingForm.markAsPristine();
        this.settingForm.markAsUntouched();
    }


    //
    // Reset form for add mode
    //
    private resetForm(): void {
        this.settingForm.reset({
            name: '',
            description: '',
            value: ''
        });
        this.settingForm.markAsPristine();
        this.settingForm.markAsUntouched();
    }


    //
    // Form submission
    //
    public submitForm(): void {

        if (this.isSaving) {
            return;
        }

        if (!this.settingForm.valid) {
            this.alertService.showMessage('Validation Error', 'Please fix form errors before saving.', MessageSeverity.warn);
            this.settingForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;

        const formValue = this.settingForm.getRawValue();

        const submitData: SystemSettingSubmitData = {
            id: this.currentSettingId ?? 0,
            name: formValue.name?.trim() ?? '',
            description: formValue.description?.trim() || null,
            value: formValue.value?.trim() || null,
            active: true,
            deleted: false
        };

        if (this.isEditMode) {
            this.updateSetting(submitData);
        } else {
            this.addSetting(submitData);
        }
    }


    //
    // Add new setting
    //
    private addSetting(submitData: SystemSettingSubmitData): void {
        this.systemSettingService.PostSystemSetting(submitData).pipe(
            finalize(() => this.isSaving = false),
            takeUntil(this.destroy$)
        ).subscribe({
            next: (newSetting) => {
                this.systemSettingService.ClearAllCaches();
                this.settingChanged.next(newSetting);
                this.alertService.showMessage('Success', 'System setting created successfully.', MessageSeverity.success);
                this.closeModal();
            },
            error: (err) => {
                this.handleError(err, 'create');
            }
        });
    }


    //
    // Update existing setting
    //
    private updateSetting(submitData: SystemSettingSubmitData): void {
        this.systemSettingService.PutSystemSetting(submitData.id, submitData).pipe(
            finalize(() => this.isSaving = false),
            takeUntil(this.destroy$)
        ).subscribe({
            next: (updatedSetting) => {
                this.systemSettingService.ClearAllCaches();
                this.settingChanged.next(updatedSetting);
                this.alertService.showMessage('Success', 'System setting updated successfully.', MessageSeverity.success);
                this.closeModal();
            },
            error: (err) => {
                this.handleError(err, 'update');
            }
        });
    }


    //
    // Error handling
    //
    private handleError(err: any, action: string): void {
        let errorMessage = `Failed to ${action} system setting.`;

        if (err instanceof Error) {
            errorMessage = err.message;
        } else if (err.status === 403) {
            errorMessage = 'You do not have permission to perform this action.';
        } else if (err.error?.message) {
            errorMessage = err.error.message;
        } else if (err.error?.detail) {
            errorMessage = err.error.detail;
        }

        this.alertService.showMessage('Error', errorMessage, MessageSeverity.error);
    }


    //
    // Form helpers
    //
    public get f() {
        return this.settingForm.controls;
    }

    public isFieldInvalid(fieldName: string): boolean {
        const field = this.settingForm.get(fieldName);
        return field != null && field.invalid && (field.dirty || field.touched);
    }

    public getFieldError(fieldName: string): string {
        const field = this.settingForm.get(fieldName);
        if (field == null || !field.errors) return '';

        if (field.errors['required']) return 'This field is required.';
        if (field.errors['maxlength']) {
            const max = field.errors['maxlength'].requiredLength;
            return `Maximum length is ${max} characters.`;
        }
        return 'Invalid value.';
    }
}
