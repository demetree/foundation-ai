/*
   GENERATED FORM FOR THE SERVICE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Service table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to service-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ServiceService, ServiceData, ServiceSubmitData } from '../../../alerting-data-services/service.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { EscalationPolicyService } from '../../../alerting-data-services/escalation-policy.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ServiceFormValues {
  escalationPolicyId: number | bigint | null,       // For FK link number
  name: string,
  description: string | null,
  ownerTeamObjectGuid: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-service-add-edit',
  templateUrl: './service-add-edit.component.html',
  styleUrls: ['./service-add-edit.component.scss']
})
export class ServiceAddEditComponent {
  @ViewChild('serviceModal') serviceModal!: TemplateRef<any>;
  @Output() serviceChanged = new Subject<ServiceData[]>();
  @Input() serviceSubmitData: ServiceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ServiceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public serviceForm: FormGroup = this.fb.group({
        escalationPolicyId: [null],
        name: ['', Validators.required],
        description: [''],
        ownerTeamObjectGuid: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  services$ = this.serviceService.GetServiceList();
  escalationPolicies$ = this.escalationPolicyService.GetEscalationPolicyList();

  constructor(
    private modalService: NgbModal,
    private serviceService: ServiceService,
    private escalationPolicyService: EscalationPolicyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(serviceData?: ServiceData) {

    if (serviceData != null) {

      if (!this.serviceService.userIsAlertingServiceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Services`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.serviceSubmitData = this.serviceService.ConvertToServiceSubmitData(serviceData);
      this.isEditMode = true;
      this.objectGuid = serviceData.objectGuid;

      this.buildFormValues(serviceData);

    } else {

      if (!this.serviceService.userIsAlertingServiceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Services`,
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
        this.serviceForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.serviceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.serviceModal, {
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

    if (this.serviceService.userIsAlertingServiceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Services`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.serviceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.serviceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.serviceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const serviceSubmitData: ServiceSubmitData = {
        id: this.serviceSubmitData?.id || 0,
        escalationPolicyId: formValue.escalationPolicyId ? Number(formValue.escalationPolicyId) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        ownerTeamObjectGuid: formValue.ownerTeamObjectGuid?.trim() || null,
        versionNumber: this.serviceSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateService(serviceSubmitData);
      } else {
        this.addService(serviceSubmitData);
      }
  }

  private addService(serviceData: ServiceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    serviceData.versionNumber = 0;
    serviceData.active = true;
    serviceData.deleted = false;
    this.serviceService.PostService(serviceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newService) => {

        this.serviceService.ClearAllCaches();

        this.serviceChanged.next([newService]);

        this.alertService.showMessage("Service added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/service', newService.id]);
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
                                   'You do not have permission to save this Service.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Service.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Service could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateService(serviceData: ServiceSubmitData) {
    this.serviceService.PutService(serviceData.id, serviceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedService) => {

        this.serviceService.ClearAllCaches();

        this.serviceChanged.next([updatedService]);

        this.alertService.showMessage("Service updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Service.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Service.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Service could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(serviceData: ServiceData | null) {

    if (serviceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.serviceForm.reset({
        escalationPolicyId: null,
        name: '',
        description: '',
        ownerTeamObjectGuid: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.serviceForm.reset({
        escalationPolicyId: serviceData.escalationPolicyId,
        name: serviceData.name ?? '',
        description: serviceData.description ?? '',
        ownerTeamObjectGuid: serviceData.ownerTeamObjectGuid ?? '',
        versionNumber: serviceData.versionNumber?.toString() ?? '',
        active: serviceData.active ?? true,
        deleted: serviceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.serviceForm.markAsPristine();
    this.serviceForm.markAsUntouched();
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


  public userIsAlertingServiceReader(): boolean {
    return this.serviceService.userIsAlertingServiceReader();
  }

  public userIsAlertingServiceWriter(): boolean {
    return this.serviceService.userIsAlertingServiceWriter();
  }
}
