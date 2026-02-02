/*
   GENERATED FORM FOR THE INTEGRATIONCALLBACKINCIDENTEVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IntegrationCallbackIncidentEventType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to integration-callback-incident-event-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IntegrationCallbackIncidentEventTypeService, IntegrationCallbackIncidentEventTypeData, IntegrationCallbackIncidentEventTypeSubmitData } from '../../../alerting-data-services/integration-callback-incident-event-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IntegrationService } from '../../../alerting-data-services/integration.service';
import { IncidentEventTypeService } from '../../../alerting-data-services/incident-event-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IntegrationCallbackIncidentEventTypeFormValues {
  integrationId: number | bigint,       // For FK link number
  incidentEventTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-integration-callback-incident-event-type-add-edit',
  templateUrl: './integration-callback-incident-event-type-add-edit.component.html',
  styleUrls: ['./integration-callback-incident-event-type-add-edit.component.scss']
})
export class IntegrationCallbackIncidentEventTypeAddEditComponent {
  @ViewChild('integrationCallbackIncidentEventTypeModal') integrationCallbackIncidentEventTypeModal!: TemplateRef<any>;
  @Output() integrationCallbackIncidentEventTypeChanged = new Subject<IntegrationCallbackIncidentEventTypeData[]>();
  @Input() integrationCallbackIncidentEventTypeSubmitData: IntegrationCallbackIncidentEventTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IntegrationCallbackIncidentEventTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public integrationCallbackIncidentEventTypeForm: FormGroup = this.fb.group({
        integrationId: [null, Validators.required],
        incidentEventTypeId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  integrationCallbackIncidentEventTypes$ = this.integrationCallbackIncidentEventTypeService.GetIntegrationCallbackIncidentEventTypeList();
  integrations$ = this.integrationService.GetIntegrationList();
  incidentEventTypes$ = this.incidentEventTypeService.GetIncidentEventTypeList();

  constructor(
    private modalService: NgbModal,
    private integrationCallbackIncidentEventTypeService: IntegrationCallbackIncidentEventTypeService,
    private integrationService: IntegrationService,
    private incidentEventTypeService: IncidentEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(integrationCallbackIncidentEventTypeData?: IntegrationCallbackIncidentEventTypeData) {

    if (integrationCallbackIncidentEventTypeData != null) {

      if (!this.integrationCallbackIncidentEventTypeService.userIsAlertingIntegrationCallbackIncidentEventTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Integration Callback Incident Event Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.integrationCallbackIncidentEventTypeSubmitData = this.integrationCallbackIncidentEventTypeService.ConvertToIntegrationCallbackIncidentEventTypeSubmitData(integrationCallbackIncidentEventTypeData);
      this.isEditMode = true;
      this.objectGuid = integrationCallbackIncidentEventTypeData.objectGuid;

      this.buildFormValues(integrationCallbackIncidentEventTypeData);

    } else {

      if (!this.integrationCallbackIncidentEventTypeService.userIsAlertingIntegrationCallbackIncidentEventTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Integration Callback Incident Event Types`,
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
        this.integrationCallbackIncidentEventTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.integrationCallbackIncidentEventTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.integrationCallbackIncidentEventTypeModal, {
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

    if (this.integrationCallbackIncidentEventTypeService.userIsAlertingIntegrationCallbackIncidentEventTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Integration Callback Incident Event Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.integrationCallbackIncidentEventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.integrationCallbackIncidentEventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.integrationCallbackIncidentEventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const integrationCallbackIncidentEventTypeSubmitData: IntegrationCallbackIncidentEventTypeSubmitData = {
        id: this.integrationCallbackIncidentEventTypeSubmitData?.id || 0,
        integrationId: Number(formValue.integrationId),
        incidentEventTypeId: Number(formValue.incidentEventTypeId),
        versionNumber: this.integrationCallbackIncidentEventTypeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIntegrationCallbackIncidentEventType(integrationCallbackIncidentEventTypeSubmitData);
      } else {
        this.addIntegrationCallbackIncidentEventType(integrationCallbackIncidentEventTypeSubmitData);
      }
  }

  private addIntegrationCallbackIncidentEventType(integrationCallbackIncidentEventTypeData: IntegrationCallbackIncidentEventTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    integrationCallbackIncidentEventTypeData.versionNumber = 0;
    integrationCallbackIncidentEventTypeData.active = true;
    integrationCallbackIncidentEventTypeData.deleted = false;
    this.integrationCallbackIncidentEventTypeService.PostIntegrationCallbackIncidentEventType(integrationCallbackIncidentEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIntegrationCallbackIncidentEventType) => {

        this.integrationCallbackIncidentEventTypeService.ClearAllCaches();

        this.integrationCallbackIncidentEventTypeChanged.next([newIntegrationCallbackIncidentEventType]);

        this.alertService.showMessage("Integration Callback Incident Event Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/integrationcallbackincidenteventtype', newIntegrationCallbackIncidentEventType.id]);
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
                                   'You do not have permission to save this Integration Callback Incident Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Integration Callback Incident Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Integration Callback Incident Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIntegrationCallbackIncidentEventType(integrationCallbackIncidentEventTypeData: IntegrationCallbackIncidentEventTypeSubmitData) {
    this.integrationCallbackIncidentEventTypeService.PutIntegrationCallbackIncidentEventType(integrationCallbackIncidentEventTypeData.id, integrationCallbackIncidentEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIntegrationCallbackIncidentEventType) => {

        this.integrationCallbackIncidentEventTypeService.ClearAllCaches();

        this.integrationCallbackIncidentEventTypeChanged.next([updatedIntegrationCallbackIncidentEventType]);

        this.alertService.showMessage("Integration Callback Incident Event Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Integration Callback Incident Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Integration Callback Incident Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Integration Callback Incident Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(integrationCallbackIncidentEventTypeData: IntegrationCallbackIncidentEventTypeData | null) {

    if (integrationCallbackIncidentEventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.integrationCallbackIncidentEventTypeForm.reset({
        integrationId: null,
        incidentEventTypeId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.integrationCallbackIncidentEventTypeForm.reset({
        integrationId: integrationCallbackIncidentEventTypeData.integrationId,
        incidentEventTypeId: integrationCallbackIncidentEventTypeData.incidentEventTypeId,
        versionNumber: integrationCallbackIncidentEventTypeData.versionNumber?.toString() ?? '',
        active: integrationCallbackIncidentEventTypeData.active ?? true,
        deleted: integrationCallbackIncidentEventTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.integrationCallbackIncidentEventTypeForm.markAsPristine();
    this.integrationCallbackIncidentEventTypeForm.markAsUntouched();
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


  public userIsAlertingIntegrationCallbackIncidentEventTypeReader(): boolean {
    return this.integrationCallbackIncidentEventTypeService.userIsAlertingIntegrationCallbackIncidentEventTypeReader();
  }

  public userIsAlertingIntegrationCallbackIncidentEventTypeWriter(): boolean {
    return this.integrationCallbackIncidentEventTypeService.userIsAlertingIntegrationCallbackIncidentEventTypeWriter();
  }
}
