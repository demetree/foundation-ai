import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerGroupService, VolunteerGroupData, VolunteerGroupSubmitData } from '../../../scheduler-data-services/volunteer-group.service';
import { VolunteerStatusService } from '../../../scheduler-data-services/volunteer-status.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-volunteer-group-custom-add-edit',
    templateUrl: './volunteer-group-custom-add-edit.component.html',
    styleUrls: ['./volunteer-group-custom-add-edit.component.scss'],
    animations: [
        trigger('collapse', [
            state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
            state('true', style({ height: '*', opacity: 1 })),
            transition('false <=> true', animate('300ms ease-in-out'))
        ])
    ]
})
export class VolunteerGroupCustomAddEditComponent {
    @ViewChild('groupModal') groupModal!: TemplateRef<any>;
    @Output() volunteerGroupChanged = new Subject<VolunteerGroupData[]>();
    @Input() volunteerGroupData: VolunteerGroupData | null = null;
    @Input() navigateToDetailsAfterAdd: boolean = true;
    @Input() showAddButton: boolean = true;

    public currentData: VolunteerGroupData | null = null;
    public isAppearancePanelOpen = false;

    groupForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        purpose: [''],
        officeId: [null],
        volunteerStatusId: [null],
        maxMembers: [null],
        iconId: [null],
        color: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
    });

    private modalRef: NgbModalRef | undefined;
    public isEditMode = false;
    public objectGuid: string = "";
    public modalIsDisplayed: boolean = false;
    public isSaving: boolean = false;

    // Avatar
    public avatarPreviewUrl: string | null = null;
    public avatarFile: File | null = null;
    public isDragOver: boolean = false;

    offices$ = this.officeService.GetOfficeList();
    volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
    icons$ = this.iconService.GetIconList();

    private submitData: VolunteerGroupSubmitData | null = null;

    constructor(
        private modalService: NgbModal,
        private volunteerGroupService: VolunteerGroupService,
        private volunteerStatusService: VolunteerStatusService,
        private officeService: OfficeService,
        private iconService: IconService,
        private authService: AuthService,
        private alertService: AlertService,
        private router: Router,
        private fb: FormBuilder
    ) { }

    openModal(data?: VolunteerGroupData) {
        if (data != null) {
            if (!this.volunteerGroupService.userIsSchedulerVolunteerGroupReader()) {
                this.alertService.showMessage(`${this.authService.currentUser?.userName} does not have permission to read Volunteer Groups`, '', MessageSeverity.info);
                return;
            }
            this.currentData = data;
            this.submitData = this.volunteerGroupService.ConvertToVolunteerGroupSubmitData(data);
            this.isEditMode = true;
            this.objectGuid = data.objectGuid;
            this.buildFormValues(data);
            this.avatarPreviewUrl = data.avatarData ? `data:${data.avatarMimeType};base64,${data.avatarData}` : null;
        } else {
            if (!this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter()) {
                this.alertService.showMessage(`${this.authService.currentUser?.userName} does not have permission to write Volunteer Groups`, '', MessageSeverity.info);
                return;
            }
            this.isEditMode = false;
            this.currentData = null;
            this.avatarPreviewUrl = null;
            this.avatarFile = null;
            this.buildFormValues(null);
        }

        this.modalRef = this.modalService.open(this.groupModal, {
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
        this.isAppearancePanelOpen = false;
        this.modalIsDisplayed = false;
    }

    submitForm() {
        if (this.isSaving) return;

        if (!this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter()) {
            this.alertService.showMessage(`${this.authService.currentUser?.userName} does not have permission to write Volunteer Groups`, '', MessageSeverity.info);
            return;
        }

        if (!this.groupForm.valid) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.groupForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;
        const formValue = this.groupForm.getRawValue();

        const data: VolunteerGroupSubmitData = {
            id: this.submitData?.id || 0,
            name: formValue.name?.trim(),
            description: formValue.description?.trim() || null,
            purpose: formValue.purpose?.trim() || null,
            officeId: formValue.officeId ? Number(formValue.officeId) : null,
            volunteerStatusId: formValue.volunteerStatusId ? Number(formValue.volunteerStatusId) : null,
            maxMembers: formValue.maxMembers ? Number(formValue.maxMembers) : null,
            iconId: formValue.iconId ? Number(formValue.iconId) : null,
            color: formValue.color?.trim() || null,
            notes: formValue.notes?.trim() || null,
            avatarFileName: this.submitData?.avatarFileName || null,
            avatarSize: this.submitData?.avatarSize || null,
            avatarData: this.submitData?.avatarData || null,
            avatarMimeType: this.submitData?.avatarMimeType || null,
            versionNumber: this.submitData?.versionNumber ?? 0,
            active: !!formValue.active,
            deleted: !!formValue.deleted,
        };

        // Handle avatar upload
        if (this.avatarFile) {
            const reader = new FileReader();
            reader.onload = (e) => {
                const base64 = (e.target?.result as string).split(',')[1];
                data.avatarData = base64;
                data.avatarFileName = this.avatarFile!.name;
                data.avatarSize = this.avatarFile!.size;
                data.avatarMimeType = this.avatarFile!.type;
                this.saveGroup(data);
            };
            reader.readAsDataURL(this.avatarFile);
        } else {
            this.saveGroup(data);
        }
    }

    private saveGroup(data: VolunteerGroupSubmitData): void {
        if (this.isEditMode) {
            this.volunteerGroupService.PutVolunteerGroup(data.id, data).pipe(
                finalize(() => this.isSaving = false)
            ).subscribe({
                next: (updated) => {
                    this.volunteerGroupService.ClearAllCaches();
                    this.volunteerGroupChanged.next([updated]);
                    this.alertService.showMessage("Group updated successfully", '', MessageSeverity.success);
                    this.closeModal();
                },
                error: (err) => {
                    this.alertService.showMessage('Group could not be saved', err?.error?.message || 'An error occurred', MessageSeverity.error);
                }
            });
        } else {
            data.versionNumber = 0;
            data.active = true;
            data.deleted = false;
            this.volunteerGroupService.PostVolunteerGroup(data).pipe(
                finalize(() => this.isSaving = false)
            ).subscribe({
                next: (newGroup) => {
                    this.volunteerGroupService.ClearAllCaches();
                    this.volunteerGroupChanged.next([newGroup]);
                    this.alertService.showMessage("Group added successfully", '', MessageSeverity.success);
                    this.closeModal();
                    if (this.navigateToDetailsAfterAdd) {
                        this.router.navigate(['/volunteer-groups', newGroup.id]);
                    }
                },
                error: (err) => {
                    this.alertService.showMessage('Group could not be saved', err?.error?.message || 'An error occurred', MessageSeverity.error);
                }
            });
        }
    }

    private buildFormValues(data: VolunteerGroupData | null) {
        if (data == null) {
            this.groupForm.reset({
                name: '',
                description: '',
                purpose: '',
                officeId: null,
                volunteerStatusId: null,
                maxMembers: null,
                iconId: null,
                color: '',
                notes: '',
                versionNumber: '',
                active: true,
                deleted: false,
            }, { emitEvent: false });
        } else {
            this.groupForm.reset({
                name: data.name ?? '',
                description: data.description ?? '',
                purpose: data.purpose ?? '',
                officeId: data.officeId,
                volunteerStatusId: data.volunteerStatusId,
                maxMembers: data.maxMembers,
                iconId: data.iconId,
                color: data.color ?? '',
                notes: data.notes ?? '',
                versionNumber: data.versionNumber?.toString() ?? '',
                active: data.active ?? true,
                deleted: data.deleted ?? false,
            }, { emitEvent: false });
        }

        this.groupForm.markAsPristine();
        this.groupForm.markAsUntouched();
    }

    // Avatar methods
    onAvatarSelect(event: any): void {
        const file = event.target.files?.[0];
        if (file) {
            this.avatarFile = file;
            const reader = new FileReader();
            reader.onload = (e) => {
                this.avatarPreviewUrl = e.target?.result as string;
                this.groupForm.markAsDirty();
            };
            reader.readAsDataURL(file);
        }
    }

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        this.isDragOver = true;
    }

    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        this.isDragOver = false;
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        this.isDragOver = false;
        const file = event.dataTransfer?.files?.[0];
        if (file && file.type.startsWith('image/')) {
            this.avatarFile = file;
            const reader = new FileReader();
            reader.onload = (e) => {
                this.avatarPreviewUrl = e.target?.result as string;
                this.groupForm.markAsDirty();
            };
            reader.readAsDataURL(file);
        }
    }

    removeAvatar(): void {
        this.avatarPreviewUrl = null;
        this.avatarFile = null;
        if (this.submitData) {
            this.submitData.avatarData = null;
            this.submitData.avatarFileName = null;
            this.submitData.avatarSize = null;
            this.submitData.avatarMimeType = null;
        }
        this.groupForm.markAsDirty();
    }

    public userIsGroupWriter(): boolean {
        return this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter();
    }

    public userIsSchedulerAdministrator(): boolean {
        return this.authService.isSchedulerAdministrator;
    }
}
