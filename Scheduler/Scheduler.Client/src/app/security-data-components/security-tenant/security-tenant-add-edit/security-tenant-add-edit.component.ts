import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityTenantService, SecurityTenantData, SecurityTenantSubmitData } from '../../../security-data-services/security-tenant.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-tenant-add-edit',
  templateUrl: './security-tenant-add-edit.component.html',
  styleUrls: ['./security-tenant-add-edit.component.scss']
})
export class SecurityTenantAddEditComponent {
  @ViewChild('securityTenantModal') securityTenantModal!: TemplateRef<any>;
  @Output() securityTenantChanged = new Subject<SecurityTenantData[]>();
  @Input() securityTenantSubmitData: SecurityTenantSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityTenantForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityTenants$ = this.securityTenantService.GetSecurityTenantList();

  constructor(
    private modalService: NgbModal,
    private securityTenantService: SecurityTenantService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityTenantData?: SecurityTenantData) {

    if (securityTenantData != null) {

      if (!this.securityTenantService.userIsSecuritySecurityTenantReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Tenants`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityTenantSubmitData = this.securityTenantService.ConvertToSecurityTenantSubmitData(securityTenantData);
      this.isEditMode = true;
      this.objectGuid = securityTenantData.objectGuid;

      this.buildFormValues(securityTenantData);

    } else {

      if (!this.securityTenantService.userIsSecuritySecurityTenantWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Tenants`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityTenantModal, {
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
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.securityTenantService.userIsSecuritySecurityTenantWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Tenants`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityTenantForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityTenantForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityTenantForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityTenantSubmitData: SecurityTenantSubmitData = {
        id: this.securityTenantSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityTenant(securityTenantSubmitData);
      } else {
        this.addSecurityTenant(securityTenantSubmitData);
      }
  }

  private addSecurityTenant(securityTenantData: SecurityTenantSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityTenantData.active = true;
    securityTenantData.deleted = false;
    this.securityTenantService.PostSecurityTenant(securityTenantData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityTenant) => {

        this.securityTenantService.ClearAllCaches();

        this.securityTenantChanged.next([newSecurityTenant]);

        this.alertService.showMessage("Security Tenant added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securitytenant', newSecurityTenant.id]);
        }
      },
      error: (err) => {
            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Security Tenant.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Tenant.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Tenant could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityTenant(securityTenantData: SecurityTenantSubmitData) {
    this.securityTenantService.PutSecurityTenant(securityTenantData.id, securityTenantData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityTenant) => {

        this.securityTenantService.ClearAllCaches();

        this.securityTenantChanged.next([updatedSecurityTenant]);

        this.alertService.showMessage("Security Tenant updated successfully", '', MessageSeverity.success);

        this.closeModal();
      },
      error: (err) => {
            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Security Tenant.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Tenant.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Tenant could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityTenantData: SecurityTenantData | null) {

    if (securityTenantData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityTenantForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityTenantForm.reset({
        name: securityTenantData.name ?? '',
        description: securityTenantData.description ?? '',
        active: securityTenantData.active ?? true,
        deleted: securityTenantData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityTenantForm.markAsPristine();
    this.securityTenantForm.markAsUntouched();
  }

  public userIsSecuritySecurityTenantReader(): boolean {
    return this.securityTenantService.userIsSecuritySecurityTenantReader();
  }

  public userIsSecuritySecurityTenantWriter(): boolean {
    return this.securityTenantService.userIsSecuritySecurityTenantWriter();
  }
}
