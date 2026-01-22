import { Component, Output, EventEmitter, ViewChild, TemplateRef, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationService, SecurityOrganizationData, SecurityOrganizationSubmitData } from '../../../security-data-services/security-organization.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-organization-add-edit',
    templateUrl: './organization-add-edit.component.html',
    styleUrls: ['./organization-add-edit.component.scss']
})
export class OrganizationAddEditComponent {
    @ViewChild('modalTemplate') modalTemplate!: TemplateRef<any>;
    @Output() saved = new EventEmitter<SecurityOrganizationData>();
    @Output() closed = new EventEmitter<void>();

    private destroy$ = new Subject<void>();
    private modalRef: NgbModalRef | null = null;

    orgForm!: FormGroup;
    isEditMode: boolean = false;
    isSaving: boolean = false;
    currentOrg: SecurityOrganizationData | null = null;
    tenant: SecurityTenantData | null = null;

    constructor(
        private modalService: NgbModal,
        private fb: FormBuilder,
        private securityOrganizationService: SecurityOrganizationService,
        private alertService: AlertService
    ) {
        this.initForm();
    }

    private initForm(): void {
        this.orgForm = this.fb.group({
            name: ['', Validators.required],
            description: [''],
            active: [true]
        });
    }

    openForCreate(tenant: SecurityTenantData): void {
        this.isEditMode = false;
        this.currentOrg = null;
        this.tenant = tenant;
        this.orgForm.reset({ name: '', description: '', active: true });
        this.openModal();
    }

    openForEdit(org: SecurityOrganizationData, tenant: SecurityTenantData): void {
        this.isEditMode = true;
        this.currentOrg = org;
        this.tenant = tenant;
        this.orgForm.patchValue({
            name: org.name,
            description: org.description || '',
            active: org.active
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
        if (this.orgForm.invalid || this.isSaving) return;

        this.isSaving = true;

        const submitData = new SecurityOrganizationSubmitData();
        submitData.name = this.orgForm.value.name;
        submitData.description = this.orgForm.value.description || '';
        submitData.active = this.orgForm.value.active;
        submitData.securityTenantId = this.tenant!.id;

        if (this.isEditMode && this.currentOrg) {
            submitData.id = this.currentOrg.id;
            this.updateOrg(submitData);
        } else {
            this.createOrg(submitData);
        }
    }

    private createOrg(submitData: SecurityOrganizationSubmitData): void {
        this.securityOrganizationService.PostSecurityOrganization(submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (newOrg: SecurityOrganizationData) => {
                this.alertService.showMessage('Success', 'Organization created successfully', MessageSeverity.success);
                this.saved.emit(newOrg);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to create organization: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    private updateOrg(submitData: SecurityOrganizationSubmitData): void {
        this.securityOrganizationService.PutSecurityOrganization(submitData.id, submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (updatedOrg: SecurityOrganizationData) => {
                this.alertService.showMessage('Success', 'Organization updated successfully', MessageSeverity.success);
                this.saved.emit(updatedOrg);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                this.alertService.showStickyMessage('Error', 'Failed to update organization: ' + err.message, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}
