/*
   GENERATED FORM FOR THE SECURITYORGANIZATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityOrganization table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-organization-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityOrganizationService, SecurityOrganizationData, SecurityOrganizationSubmitData } from '../../../security-data-services/security-organization.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityTenantService } from '../../../security-data-services/security-tenant.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityOrganizationFormValues {
  securityTenantId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-security-organization-add-edit',
  templateUrl: './security-organization-add-edit.component.html',
  styleUrls: ['./security-organization-add-edit.component.scss']
})
export class SecurityOrganizationAddEditComponent {
  @ViewChild('securityOrganizationModal') securityOrganizationModal!: TemplateRef<any>;
  @Output() securityOrganizationChanged = new Subject<SecurityOrganizationData[]>();
  @Input() securityOrganizationSubmitData: SecurityOrganizationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityOrganizationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityOrganizationForm: FormGroup = this.fb.group({
        securityTenantId: [null, Validators.required],
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

  securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  securityTenants$ = this.securityTenantService.GetSecurityTenantList();

  constructor(
    private modalService: NgbModal,
    private securityOrganizationService: SecurityOrganizationService,
    private securityTenantService: SecurityTenantService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityOrganizationData?: SecurityOrganizationData) {

    if (securityOrganizationData != null) {

      if (!this.securityOrganizationService.userIsSecuritySecurityOrganizationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Organizations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityOrganizationSubmitData = this.securityOrganizationService.ConvertToSecurityOrganizationSubmitData(securityOrganizationData);
      this.isEditMode = true;
      this.objectGuid = securityOrganizationData.objectGuid;

      this.buildFormValues(securityOrganizationData);

    } else {

      if (!this.securityOrganizationService.userIsSecuritySecurityOrganizationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Organizations`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityOrganizationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityOrganizationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.securityOrganizationModal, {
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

    if (this.securityOrganizationService.userIsSecuritySecurityOrganizationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Organizations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityOrganizationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityOrganizationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityOrganizationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityOrganizationSubmitData: SecurityOrganizationSubmitData = {
        id: this.securityOrganizationSubmitData?.id || 0,
        securityTenantId: Number(formValue.securityTenantId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityOrganization(securityOrganizationSubmitData);
      } else {
        this.addSecurityOrganization(securityOrganizationSubmitData);
      }
  }

  private addSecurityOrganization(securityOrganizationData: SecurityOrganizationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityOrganizationData.active = true;
    securityOrganizationData.deleted = false;
    this.securityOrganizationService.PostSecurityOrganization(securityOrganizationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityOrganization) => {

        this.securityOrganizationService.ClearAllCaches();

        this.securityOrganizationChanged.next([newSecurityOrganization]);

        this.alertService.showMessage("Security Organization added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityorganization', newSecurityOrganization.id]);
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
                                   'You do not have permission to save this Security Organization.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Organization.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Organization could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityOrganization(securityOrganizationData: SecurityOrganizationSubmitData) {
    this.securityOrganizationService.PutSecurityOrganization(securityOrganizationData.id, securityOrganizationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityOrganization) => {

        this.securityOrganizationService.ClearAllCaches();

        this.securityOrganizationChanged.next([updatedSecurityOrganization]);

        this.alertService.showMessage("Security Organization updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Organization.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Organization.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Organization could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityOrganizationData: SecurityOrganizationData | null) {

    if (securityOrganizationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityOrganizationForm.reset({
        securityTenantId: null,
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
        this.securityOrganizationForm.reset({
        securityTenantId: securityOrganizationData.securityTenantId,
        name: securityOrganizationData.name ?? '',
        description: securityOrganizationData.description ?? '',
        active: securityOrganizationData.active ?? true,
        deleted: securityOrganizationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityOrganizationForm.markAsPristine();
    this.securityOrganizationForm.markAsUntouched();
  }

  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public userIsSecuritySecurityOrganizationReader(): boolean {
    return this.securityOrganizationService.userIsSecuritySecurityOrganizationReader();
  }

  public userIsSecuritySecurityOrganizationWriter(): boolean {
    return this.securityOrganizationService.userIsSecuritySecurityOrganizationWriter();
  }
}
