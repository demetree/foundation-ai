import { Component, Output, EventEmitter, ViewChild, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { SecurityOrganizationData } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentService, SecurityDepartmentData, SecurityDepartmentSubmitData } from '../../../security-data-services/security-department.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-department-add-edit',
    templateUrl: './department-add-edit.component.html',
    styleUrls: ['./department-add-edit.component.scss']
})
export class DepartmentAddEditComponent {
    @ViewChild('modalTemplate') modalTemplate!: TemplateRef<any>;
    @Output() saved = new EventEmitter<SecurityDepartmentData>();
    @Output() closed = new EventEmitter<void>();

    private destroy$ = new Subject<void>();
    private modalRef: NgbModalRef | null = null;

    deptForm!: FormGroup;
    isEditMode: boolean = false;
    isSaving: boolean = false;
    currentDept: SecurityDepartmentData | null = null;
    organization: SecurityOrganizationData | null = null;

    constructor(
        private modalService: NgbModal,
        private fb: FormBuilder,
        private securityDepartmentService: SecurityDepartmentService,
        private alertService: AlertService
    ) {
        this.initForm();
    }

    private initForm(): void {
        this.deptForm = this.fb.group({
            name: ['', Validators.required],
            description: [''],
            active: [true]
        });
    }

    openForCreate(org: SecurityOrganizationData): void {
        this.isEditMode = false;
        this.currentDept = null;
        this.organization = org;
        this.deptForm.reset({ name: '', description: '', active: true });
        this.openModal();
    }

    openForEdit(dept: SecurityDepartmentData, org: SecurityOrganizationData): void {
        this.isEditMode = true;
        this.currentDept = dept;
        this.organization = org;
        this.deptForm.patchValue({
            name: dept.name,
            description: dept.description || '',
            active: dept.active
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
        if (this.deptForm.invalid || this.isSaving) return;

        this.isSaving = true;

        const submitData = new SecurityDepartmentSubmitData();
        submitData.name = this.deptForm.value.name;
        submitData.description = this.deptForm.value.description || '';
        submitData.active = this.deptForm.value.active;
        submitData.securityOrganizationId = this.organization!.id;

        if (this.isEditMode && this.currentDept) {
            submitData.id = this.currentDept.id;
            this.updateDept(submitData);
        } else {
            this.createDept(submitData);
        }
    }

    private createDept(submitData: SecurityDepartmentSubmitData): void {
        this.securityDepartmentService.PostSecurityDepartment(submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (newDept: SecurityDepartmentData) => {
                this.alertService.showMessage('Success', 'Department created successfully', MessageSeverity.success);
                this.saved.emit(newDept);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to create department: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    private updateDept(submitData: SecurityDepartmentSubmitData): void {
        this.securityDepartmentService.PutSecurityDepartment(submitData.id, submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (updatedDept: SecurityDepartmentData) => {
                this.alertService.showMessage('Success', 'Department updated successfully', MessageSeverity.success);
                this.saved.emit(updatedDept);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to update department: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}
