import { Component, ViewChild, Output, Input, TemplateRef, SimpleChanges } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerProfileService, VolunteerProfileData, VolunteerProfileSubmitData } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerStatusService } from '../../../scheduler-data-services/volunteer-status.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';

/**
 * Converts an ISO date string (e.g. "2024-01-15" or "2024-01-15T00:00:00Z") to YYYY-MM-DD for <input type="date">.
 * Returns empty string if input is null/undefined/empty.
 */
function isoToDateInput(value: string | null | undefined): string {
    if (!value) return '';
    return value.substring(0, 10); // Take just YYYY-MM-DD
}

/**
 * Converts a date input value (YYYY-MM-DD) to the format the server expects for DateOnly fields.
 * Returns null if input is empty.
 */
function dateInputToDateOnly(value: string | null | undefined): string | null {
    if (!value || value.trim() === '') return null;
    return value; // Already in YYYY-MM-DD format from <input type="date">
}

@Component({
    selector: 'app-volunteer-custom-add-edit',
    templateUrl: './volunteer-custom-add-edit.component.html',
    styleUrls: ['./volunteer-custom-add-edit.component.scss'],
    animations: [
        trigger('collapse', [
            state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
            state('true', style({ height: '*', opacity: 1 })),
            transition('false <=> true', animate('300ms ease-in-out'))
        ])
    ]
})
export class VolunteerCustomAddEditComponent {
    @ViewChild('volunteerModal') volunteerModal!: TemplateRef<any>;
    @Output() volunteerProfileChanged = new Subject<VolunteerProfileData[]>();
    @Input() volunteerProfileData: VolunteerProfileData | null = null;
    @Input() navigateToDetailsAfterAdd: boolean = true;
    @Input() showAddButton: boolean = true;

    public currentData: VolunteerProfileData | null = null;

    public isCompliancePanelOpen = false;
    public isNotesPanelOpen = false;
    public isAppearancePanelOpen = false;

    volunteerForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        volunteerStatusId: [null, Validators.required],
        onboardedDate: [''],
        inactiveSince: [''],
        totalHoursServed: [0],
        lastActivityDate: [''],
        backgroundCheckCompleted: [false],
        backgroundCheckDate: [''],
        backgroundCheckExpiry: [''],
        confidentialityAgreementSigned: [false],
        confidentialityAgreementDate: [''],
        availabilityPreferences: [''],
        interestsAndSkillsNotes: [''],
        emergencyContactNotes: [''],
        constituentId: [null],
        iconId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
    });

    private modalRef: NgbModalRef | undefined;
    public isEditMode = false;
    public objectGuid: string = "";
    public modalIsDisplayed: boolean = false;
    public isSaving: boolean = false;

    resources$ = this.resourceService.GetResourceList();
    volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
    icons$ = this.iconService.GetIconList();

    private submitData: VolunteerProfileSubmitData | null = null;

    constructor(
        private modalService: NgbModal,
        private volunteerProfileService: VolunteerProfileService,
        private volunteerStatusService: VolunteerStatusService,
        private resourceService: ResourceService,
        private iconService: IconService,
        private authService: AuthService,
        private alertService: AlertService,
        private currentUserService: CurrentUserService,
        private router: Router,
        private fb: FormBuilder
    ) { }

    openModal(data?: VolunteerProfileData) {
        if (data != null) {
            if (!this.volunteerProfileService.userIsSchedulerVolunteerProfileReader()) {
                this.alertService.showMessage(
                    `${this.authService.currentUser?.userName} does not have permission to read Volunteers`,
                    '', MessageSeverity.info
                );
                return;
            }

            this.currentData = data;
            this.submitData = this.volunteerProfileService.ConvertToVolunteerProfileSubmitData(data);
            this.isEditMode = true;
            this.objectGuid = data.objectGuid;
            this.buildFormValues(data);
        } else {
            if (!this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter()) {
                this.alertService.showMessage(
                    `${this.authService.currentUser?.userName} does not have permission to write Volunteers`,
                    '', MessageSeverity.info
                );
                return;
            }

            this.isEditMode = false;
            this.currentData = null;
            this.buildFormValues(null);
        }

        this.modalRef = this.modalService.open(this.volunteerModal, {
            size: 'xl',
            scrollable: true,
            backdrop: 'static',
            keyboard: true,
            windowClass: 'custom-modal'
        });
        this.modalIsDisplayed = true;
    }

    closeModal() {
        if (this.modalRef) {
            this.modalRef.dismiss('cancel');
        }
        this.isCompliancePanelOpen = false;
        this.isNotesPanelOpen = false;
        this.isAppearancePanelOpen = false;
        this.modalIsDisplayed = false;
    }

    submitForm() {
        if (this.isSaving) return;

        if (!this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter()) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to write Volunteers`,
                '', MessageSeverity.info
            );
            return;
        }

        if (!this.volunteerForm.valid) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.volunteerForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const formValue = this.volunteerForm.getRawValue();

        const data: VolunteerProfileSubmitData = {
            id: this.submitData?.id || 0,
            resourceId: Number(formValue.resourceId),
            volunteerStatusId: Number(formValue.volunteerStatusId),
            onboardedDate: dateInputToDateOnly(formValue.onboardedDate),
            inactiveSince: dateInputToDateOnly(formValue.inactiveSince),
            totalHoursServed: formValue.totalHoursServed ? Number(formValue.totalHoursServed) : null,
            lastActivityDate: dateInputToDateOnly(formValue.lastActivityDate),
            backgroundCheckCompleted: !!formValue.backgroundCheckCompleted,
            backgroundCheckDate: dateInputToDateOnly(formValue.backgroundCheckDate),
            backgroundCheckExpiry: dateInputToDateOnly(formValue.backgroundCheckExpiry),
            confidentialityAgreementSigned: !!formValue.confidentialityAgreementSigned,
            confidentialityAgreementDate: dateInputToDateOnly(formValue.confidentialityAgreementDate),
            availabilityPreferences: formValue.availabilityPreferences?.trim() || null,
            interestsAndSkillsNotes: formValue.interestsAndSkillsNotes?.trim() || null,
            emergencyContactNotes: formValue.emergencyContactNotes?.trim() || null,
            constituentId: formValue.constituentId ? Number(formValue.constituentId) : null,
            iconId: formValue.iconId ? Number(formValue.iconId) : null,
            color: formValue.color?.trim() || null,
            attributes: null,
            versionNumber: this.submitData?.versionNumber ?? 0,
            active: !!formValue.active,
            deleted: !!formValue.deleted,
        };

        if (this.isEditMode) {
            this.updateVolunteer(data);
        } else {
            this.addVolunteer(data);
        }
    }

    private addVolunteer(data: VolunteerProfileSubmitData) {
        data.versionNumber = 0;
        data.active = true;
        data.deleted = false;
        this.volunteerProfileService.PostVolunteerProfile(data).pipe(
            finalize(() => this.isSaving = false)
        ).subscribe({
            next: (newProfile) => {
                this.volunteerProfileService.ClearAllCaches();
                this.volunteerProfileChanged.next([newProfile]);
                this.alertService.showMessage("Volunteer added successfully", '', MessageSeverity.success);
                this.closeModal();
                if (this.navigateToDetailsAfterAdd) {
                    this.router.navigate(['/volunteers', newProfile.id]);
                }
            },
            error: (err) => {
                let errorMessage = this.extractErrorMessage(err, 'Volunteer');
                this.alertService.showMessage('Volunteer could not be saved', errorMessage, MessageSeverity.error);
            }
        });
    }

    private updateVolunteer(data: VolunteerProfileSubmitData) {
        this.volunteerProfileService.PutVolunteerProfile(data.id, data).pipe(
            finalize(() => this.isSaving = false)
        ).subscribe({
            next: (updatedProfile) => {
                this.volunteerProfileService.ClearAllCaches();
                this.volunteerProfileChanged.next([updatedProfile]);
                this.alertService.showMessage("Volunteer updated successfully", '', MessageSeverity.success);
                this.closeModal();
            },
            error: (err) => {
                let errorMessage = this.extractErrorMessage(err, 'Volunteer');
                this.alertService.showMessage('Volunteer could not be saved', errorMessage, MessageSeverity.error);
            }
        });
    }

    private extractErrorMessage(err: any, entityName: string): string {
        if (err instanceof Error) {
            return err.message || 'An unexpected error occurred.';
        } else if (err.status && err.error) {
            if (err.status === 403) {
                return err.error?.message || `You do not have permission to save this ${entityName}.`;
            }
            return err.error?.message || err.error?.error_description || err.error?.detail || `An error occurred while saving the ${entityName}.`;
        }
        return 'An unexpected error occurred.';
    }

    private buildFormValues(data: VolunteerProfileData | null) {
        if (data == null) {
            this.volunteerForm.reset({
                resourceId: null,
                volunteerStatusId: null,
                onboardedDate: '',
                inactiveSince: '',
                totalHoursServed: 0,
                lastActivityDate: '',
                backgroundCheckCompleted: false,
                backgroundCheckDate: '',
                backgroundCheckExpiry: '',
                confidentialityAgreementSigned: false,
                confidentialityAgreementDate: '',
                availabilityPreferences: '',
                interestsAndSkillsNotes: '',
                emergencyContactNotes: '',
                constituentId: null,
                iconId: null,
                color: '',
                versionNumber: '',
                active: true,
                deleted: false,
            }, { emitEvent: false });
        } else {
            this.volunteerForm.reset({
                resourceId: data.resourceId,
                volunteerStatusId: data.volunteerStatusId,
                onboardedDate: isoToDateInput(data.onboardedDate),
                inactiveSince: isoToDateInput(data.inactiveSince),
                totalHoursServed: data.totalHoursServed ?? 0,
                lastActivityDate: isoToDateInput(data.lastActivityDate),
                backgroundCheckCompleted: data.backgroundCheckCompleted ?? false,
                backgroundCheckDate: isoToDateInput(data.backgroundCheckDate),
                backgroundCheckExpiry: isoToDateInput(data.backgroundCheckExpiry),
                confidentialityAgreementSigned: data.confidentialityAgreementSigned ?? false,
                confidentialityAgreementDate: isoToDateInput(data.confidentialityAgreementDate),
                availabilityPreferences: data.availabilityPreferences ?? '',
                interestsAndSkillsNotes: data.interestsAndSkillsNotes ?? '',
                emergencyContactNotes: data.emergencyContactNotes ?? '',
                constituentId: data.constituentId,
                iconId: data.iconId,
                color: data.color ?? '',
                versionNumber: data.versionNumber?.toString() ?? '',
                active: data.active ?? true,
                deleted: data.deleted ?? false,
            }, { emitEvent: false });
        }

        this.volunteerForm.markAsPristine();
        this.volunteerForm.markAsUntouched();
    }

    public userIsVolunteerReader(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileReader();
    }

    public userIsVolunteerWriter(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
    }

    public userIsSchedulerAdministrator(): boolean {
        return this.authService.isSchedulerAdministrator;
    }
}
