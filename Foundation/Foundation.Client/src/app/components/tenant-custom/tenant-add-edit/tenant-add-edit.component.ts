//
// Tenant Add/Edit Component
//
// AI-Developed: Modal component for creating and editing Security Tenants.
// Follows the organization-add-edit modal pattern with NgbModal, FormGroup,
// and SecurityTenantService CRUD operations.
//

import { Component, Output, EventEmitter, ViewChild, TemplateRef, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { SecurityTenantService, SecurityTenantData, SecurityTenantSubmitData } from '../../../security-data-services/security-tenant.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-tenant-add-edit',
    templateUrl: './tenant-add-edit.component.html',
    styleUrls: ['./tenant-add-edit.component.scss']
})
export class TenantAddEditComponent implements OnDestroy {

    @ViewChild('modalTemplate') modalTemplate!: TemplateRef<any>;
    @Output() saved = new EventEmitter<SecurityTenantData>();
    @Output() closed = new EventEmitter<void>();

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();
    private modalRef: NgbModalRef | null = null;

    //
    // Form and state
    //
    tenantForm!: FormGroup;
    isEditMode: boolean = false;
    isSaving: boolean = false;
    currentTenant: SecurityTenantData | null = null;


    constructor(
        private modalService: NgbModal,
        private fb: FormBuilder,
        private securityTenantService: SecurityTenantService,
        private authService: AuthService,
        private alertService: AlertService
    ) {
        this.initForm();
    }


    //
    // Form initialization
    //

    private initForm(): void {
        this.tenantForm = this.fb.group({
            name: ['', Validators.required],
            description: [''],
            hostName: [''],
            active: [true]
        });
    }


    //
    // Public methods to open the modal
    //

    openForCreate(): void {

        //
        // Permission check — only writers can create tenants
        //
        if (this.securityTenantService.userIsSecuritySecurityTenantWriter() == false) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to create Security Tenants`,
                '',
                MessageSeverity.info
            );
            return;
        }

        this.isEditMode = false;
        this.currentTenant = null;
        this.tenantForm.reset({ name: '', description: '', hostName: '', active: true });
        this.openModal();
    }


    openForEdit(tenant: SecurityTenantData): void {

        //
        // Permission check — only readers can view; writers can edit
        //
        if (this.securityTenantService.userIsSecuritySecurityTenantWriter() == false) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to edit Security Tenants`,
                '',
                MessageSeverity.info
            );
            return;
        }

        this.isEditMode = true;
        this.currentTenant = tenant;
        this.tenantForm.patchValue({
            name: tenant.name,
            description: tenant.description || '',
            hostName: tenant.hostName || '',
            active: tenant.active
        });
        this.openModal();
    }


    //
    // Modal management
    //

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


    //
    // Form submission
    //

    submitForm(): void {

        if (this.isSaving == true) {
            return;
        }

        if (this.tenantForm.invalid == true) {
            this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
            this.tenantForm.markAllAsTouched();
            return;
        }

        this.isSaving = true;

        //
        // Build clean submit object from form values
        //
        let submitData = new SecurityTenantSubmitData();
        submitData.name = this.tenantForm.value.name.trim();
        submitData.description = this.tenantForm.value.description?.trim() || null;
        submitData.hostName = this.tenantForm.value.hostName?.trim() || null;
        submitData.active = this.tenantForm.value.active;
        submitData.deleted = false;

        if (this.isEditMode == true && this.currentTenant != null) {
            submitData.id = this.currentTenant.id;
            submitData.settings = this.currentTenant.settings;
            this.updateTenant(submitData);
        } else {
            submitData.settings = null;
            this.createTenant(submitData);
        }
    }


    //
    // Create a new tenant
    //

    private createTenant(submitData: SecurityTenantSubmitData): void {

        this.securityTenantService.PostSecurityTenant(submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (newTenant: SecurityTenantData) => {

                //
                // Clear caches so listings refresh with the new tenant
                //
                this.securityTenantService.ClearAllCaches();

                this.alertService.showMessage('Success', 'Tenant created successfully', MessageSeverity.success);
                this.saved.emit(newTenant);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                let errorMessage = 'An unexpected error occurred.';

                if (err instanceof Error) {
                    errorMessage = err.message || errorMessage;
                } else if (err.status && err.error) {
                    if (err.status === 403) {
                        errorMessage = err.error?.message || 'You do not have permission to create this Tenant.';
                    } else {
                        errorMessage = err.error?.message ||
                                       err.error?.error_description ||
                                       err.error?.detail ||
                                       'An error occurred while creating the Tenant.';
                    }
                }

                this.alertService.showStickyMessage('Error', 'Failed to create tenant: ' + errorMessage, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }


    //
    // Update an existing tenant
    //

    private updateTenant(submitData: SecurityTenantSubmitData): void {

        this.securityTenantService.PutSecurityTenant(submitData.id, submitData).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (updatedTenant: SecurityTenantData) => {

                //
                // Clear caches so listings refresh with the updated tenant
                //
                this.securityTenantService.ClearAllCaches();

                this.alertService.showMessage('Success', 'Tenant updated successfully', MessageSeverity.success);
                this.saved.emit(updatedTenant);
                this.isSaving = false;
                this.closeModal();
            },
            error: (err: any) => {
                let errorMessage = 'An unexpected error occurred.';

                if (err instanceof Error) {
                    errorMessage = err.message || errorMessage;
                } else if (err.status && err.error) {
                    if (err.status === 403) {
                        errorMessage = err.error?.message || 'You do not have permission to edit this Tenant.';
                    } else {
                        errorMessage = err.error?.message ||
                                       err.error?.error_description ||
                                       err.error?.detail ||
                                       'An error occurred while updating the Tenant.';
                    }
                }

                this.alertService.showStickyMessage('Error', 'Failed to update tenant: ' + errorMessage, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }


    //
    // Cleanup
    //

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}
