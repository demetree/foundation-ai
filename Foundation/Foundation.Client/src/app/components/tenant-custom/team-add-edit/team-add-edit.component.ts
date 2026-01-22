import { Component, Output, EventEmitter, ViewChild, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { SecurityDepartmentData } from '../../../security-data-services/security-department.service';
import { SecurityTeamService, SecurityTeamData, SecurityTeamSubmitData } from '../../../security-data-services/security-team.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-team-add-edit',
    templateUrl: './team-add-edit.component.html',
    styleUrls: ['./team-add-edit.component.scss']
})
export class TeamAddEditComponent {
    @ViewChild('modalTemplate') modalTemplate!: TemplateRef<any>;
    @Output() saved = new EventEmitter<SecurityTeamData>();
    @Output() closed = new EventEmitter<void>();

    private destroy$ = new Subject<void>();
    private modalRef: NgbModalRef | null = null;

    teamForm!: FormGroup;
    isEditMode: boolean = false;
    isSaving: boolean = false;
    currentTeam: SecurityTeamData | null = null;
    department: SecurityDepartmentData | null = null;

    constructor(
        private modalService: NgbModal,
        private fb: FormBuilder,
        private securityTeamService: SecurityTeamService,
        private alertService: AlertService
    ) {
        this.initForm();
    }

    private initForm(): void {
        this.teamForm = this.fb.group({
            name: ['', Validators.required],
            description: [''],
            active: [true]
        });
    }

    openForCreate(dept: SecurityDepartmentData): void {
        this.isEditMode = false;
        this.currentTeam = null;
        this.department = dept;
        this.teamForm.reset({ name: '', description: '', active: true });
        this.openModal();
    }

    openForEdit(team: SecurityTeamData, dept: SecurityDepartmentData): void {
        this.isEditMode = true;
        this.currentTeam = team;
        this.department = dept;
        this.teamForm.patchValue({
            name: team.name,
            description: team.description || '',
            active: team.active
        });
        this.openModal();
    }

    private openModal(): void {
        this.modalRef = this.modalService.open(this.modalTemplate, {
            size: 'md',
            backdrop: 'static',
            centered: true
        });
    }

    closeModal(): void {
        if (this.modalRef) {
            this.modalRef.close();
            this.modalRef = null;
        }
        this.closed.emit();
    }

    submitForm(): void {
        if (this.teamForm.invalid || this.isSaving) return;

        this.isSaving = true;

        const submitData = new SecurityTeamSubmitData();
        submitData.name = this.teamForm.value.name;
        submitData.description = this.teamForm.value.description || '';
        submitData.active = this.teamForm.value.active;
        submitData.securityDepartmentId = this.department!.id;

        if (this.isEditMode && this.currentTeam) {
            submitData.id = this.currentTeam.id;
            this.updateTeam(submitData);
        } else {
            this.createTeam(submitData);
        }
    }

    private createTeam(submitData: SecurityTeamSubmitData): void {
        this.securityTeamService.PostSecurityTeam(submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (newTeam: SecurityTeamData) => {
                this.alertService.showMessage('Success', 'Team created successfully', MessageSeverity.success);
                this.saved.emit(newTeam);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to create team: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    private updateTeam(submitData: SecurityTeamSubmitData): void {
        this.securityTeamService.PutSecurityTeam(submitData.id, submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (updatedTeam: SecurityTeamData) => {
                this.alertService.showMessage('Success', 'Team updated successfully', MessageSeverity.success);
                this.saved.emit(updatedTeam);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to update team: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}
