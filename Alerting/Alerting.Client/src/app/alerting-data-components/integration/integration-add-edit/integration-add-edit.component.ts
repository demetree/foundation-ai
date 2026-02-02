/*
   GENERATED FORM FOR THE INTEGRATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Integration table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to integration-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IntegrationService, IntegrationData, IntegrationSubmitData } from '../../../alerting-data-services/integration.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ServiceService } from '../../../alerting-data-services/service.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IntegrationFormValues {
  serviceId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  apiKeyHash: string,
  callbackWebhookUrl: string | null,
  maxRetryAttempts: string | null,     // Stored as string for form input, converted to number on submit.
  retryBackoffSeconds: string | null,     // Stored as string for form input, converted to number on submit.
  lastCallbackSuccessAt: string | null,
  consecutiveCallbackFailures: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-integration-add-edit',
  templateUrl: './integration-add-edit.component.html',
  styleUrls: ['./integration-add-edit.component.scss']
})
export class IntegrationAddEditComponent {
  @ViewChild('integrationModal') integrationModal!: TemplateRef<any>;
  @Output() integrationChanged = new Subject<IntegrationData[]>();
  @Input() integrationSubmitData: IntegrationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IntegrationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public integrationForm: FormGroup = this.fb.group({
        serviceId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        apiKeyHash: ['', Validators.required],
        callbackWebhookUrl: [''],
        maxRetryAttempts: [''],
        retryBackoffSeconds: [''],
        lastCallbackSuccessAt: [''],
        consecutiveCallbackFailures: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  integrations$ = this.integrationService.GetIntegrationList();
  services$ = this.serviceService.GetServiceList();

  constructor(
    private modalService: NgbModal,
    private integrationService: IntegrationService,
    private serviceService: ServiceService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(integrationData?: IntegrationData) {

    if (integrationData != null) {

      if (!this.integrationService.userIsAlertingIntegrationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Integrations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.integrationSubmitData = this.integrationService.ConvertToIntegrationSubmitData(integrationData);
      this.isEditMode = true;
      this.objectGuid = integrationData.objectGuid;

      this.buildFormValues(integrationData);

    } else {

      if (!this.integrationService.userIsAlertingIntegrationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Integrations`,
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
        this.integrationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.integrationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.integrationModal, {
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

    if (this.integrationService.userIsAlertingIntegrationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Integrations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.integrationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.integrationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.integrationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const integrationSubmitData: IntegrationSubmitData = {
        id: this.integrationSubmitData?.id || 0,
        serviceId: Number(formValue.serviceId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        apiKeyHash: formValue.apiKeyHash!.trim(),
        callbackWebhookUrl: formValue.callbackWebhookUrl?.trim() || null,
        maxRetryAttempts: formValue.maxRetryAttempts ? Number(formValue.maxRetryAttempts) : null,
        retryBackoffSeconds: formValue.retryBackoffSeconds ? Number(formValue.retryBackoffSeconds) : null,
        lastCallbackSuccessAt: formValue.lastCallbackSuccessAt ? dateTimeLocalToIsoUtc(formValue.lastCallbackSuccessAt.trim()) : null,
        consecutiveCallbackFailures: formValue.consecutiveCallbackFailures ? Number(formValue.consecutiveCallbackFailures) : null,
        versionNumber: this.integrationSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIntegration(integrationSubmitData);
      } else {
        this.addIntegration(integrationSubmitData);
      }
  }

  private addIntegration(integrationData: IntegrationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    integrationData.versionNumber = 0;
    integrationData.active = true;
    integrationData.deleted = false;
    this.integrationService.PostIntegration(integrationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIntegration) => {

        this.integrationService.ClearAllCaches();

        this.integrationChanged.next([newIntegration]);

        this.alertService.showMessage("Integration added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/integration', newIntegration.id]);
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
                                   'You do not have permission to save this Integration.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Integration.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Integration could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIntegration(integrationData: IntegrationSubmitData) {
    this.integrationService.PutIntegration(integrationData.id, integrationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIntegration) => {

        this.integrationService.ClearAllCaches();

        this.integrationChanged.next([updatedIntegration]);

        this.alertService.showMessage("Integration updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Integration.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Integration.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Integration could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(integrationData: IntegrationData | null) {

    if (integrationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.integrationForm.reset({
        serviceId: null,
        name: '',
        description: '',
        apiKeyHash: '',
        callbackWebhookUrl: '',
        maxRetryAttempts: '',
        retryBackoffSeconds: '',
        lastCallbackSuccessAt: '',
        consecutiveCallbackFailures: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.integrationForm.reset({
        serviceId: integrationData.serviceId,
        name: integrationData.name ?? '',
        description: integrationData.description ?? '',
        apiKeyHash: integrationData.apiKeyHash ?? '',
        callbackWebhookUrl: integrationData.callbackWebhookUrl ?? '',
        maxRetryAttempts: integrationData.maxRetryAttempts?.toString() ?? '',
        retryBackoffSeconds: integrationData.retryBackoffSeconds?.toString() ?? '',
        lastCallbackSuccessAt: isoUtcStringToDateTimeLocal(integrationData.lastCallbackSuccessAt) ?? '',
        consecutiveCallbackFailures: integrationData.consecutiveCallbackFailures?.toString() ?? '',
        versionNumber: integrationData.versionNumber?.toString() ?? '',
        active: integrationData.active ?? true,
        deleted: integrationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.integrationForm.markAsPristine();
    this.integrationForm.markAsUntouched();
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


  public userIsAlertingIntegrationReader(): boolean {
    return this.integrationService.userIsAlertingIntegrationReader();
  }

  public userIsAlertingIntegrationWriter(): boolean {
    return this.integrationService.userIsAlertingIntegrationWriter();
  }
}
