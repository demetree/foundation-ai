/*
   GENERATED FORM FOR THE INTEGRATIONCALLBACKINCIDENTEVENTTYPECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IntegrationCallbackIncidentEventTypeChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to integration-callback-incident-event-type-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IntegrationCallbackIncidentEventTypeChangeHistoryService, IntegrationCallbackIncidentEventTypeChangeHistoryData, IntegrationCallbackIncidentEventTypeChangeHistorySubmitData } from '../../../alerting-data-services/integration-callback-incident-event-type-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IntegrationCallbackIncidentEventTypeService } from '../../../alerting-data-services/integration-callback-incident-event-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IntegrationCallbackIncidentEventTypeChangeHistoryFormValues {
  integrationCallbackIncidentEventTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-integration-callback-incident-event-type-change-history-add-edit',
  templateUrl: './integration-callback-incident-event-type-change-history-add-edit.component.html',
  styleUrls: ['./integration-callback-incident-event-type-change-history-add-edit.component.scss']
})
export class IntegrationCallbackIncidentEventTypeChangeHistoryAddEditComponent {
  @ViewChild('integrationCallbackIncidentEventTypeChangeHistoryModal') integrationCallbackIncidentEventTypeChangeHistoryModal!: TemplateRef<any>;
  @Output() integrationCallbackIncidentEventTypeChangeHistoryChanged = new Subject<IntegrationCallbackIncidentEventTypeChangeHistoryData[]>();
  @Input() integrationCallbackIncidentEventTypeChangeHistorySubmitData: IntegrationCallbackIncidentEventTypeChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IntegrationCallbackIncidentEventTypeChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public integrationCallbackIncidentEventTypeChangeHistoryForm: FormGroup = this.fb.group({
        integrationCallbackIncidentEventTypeId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  integrationCallbackIncidentEventTypeChangeHistories$ = this.integrationCallbackIncidentEventTypeChangeHistoryService.GetIntegrationCallbackIncidentEventTypeChangeHistoryList();
  integrationCallbackIncidentEventTypes$ = this.integrationCallbackIncidentEventTypeService.GetIntegrationCallbackIncidentEventTypeList();

  constructor(
    private modalService: NgbModal,
    private integrationCallbackIncidentEventTypeChangeHistoryService: IntegrationCallbackIncidentEventTypeChangeHistoryService,
    private integrationCallbackIncidentEventTypeService: IntegrationCallbackIncidentEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(integrationCallbackIncidentEventTypeChangeHistoryData?: IntegrationCallbackIncidentEventTypeChangeHistoryData) {

    if (integrationCallbackIncidentEventTypeChangeHistoryData != null) {

      if (!this.integrationCallbackIncidentEventTypeChangeHistoryService.userIsAlertingIntegrationCallbackIncidentEventTypeChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Integration Callback Incident Event Type Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.integrationCallbackIncidentEventTypeChangeHistorySubmitData = this.integrationCallbackIncidentEventTypeChangeHistoryService.ConvertToIntegrationCallbackIncidentEventTypeChangeHistorySubmitData(integrationCallbackIncidentEventTypeChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(integrationCallbackIncidentEventTypeChangeHistoryData);

    } else {

      if (!this.integrationCallbackIncidentEventTypeChangeHistoryService.userIsAlertingIntegrationCallbackIncidentEventTypeChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Integration Callback Incident Event Type Change Histories`,
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
        this.integrationCallbackIncidentEventTypeChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.integrationCallbackIncidentEventTypeChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.integrationCallbackIncidentEventTypeChangeHistoryModal, {
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

    if (this.integrationCallbackIncidentEventTypeChangeHistoryService.userIsAlertingIntegrationCallbackIncidentEventTypeChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Integration Callback Incident Event Type Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.integrationCallbackIncidentEventTypeChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.integrationCallbackIncidentEventTypeChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.integrationCallbackIncidentEventTypeChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const integrationCallbackIncidentEventTypeChangeHistorySubmitData: IntegrationCallbackIncidentEventTypeChangeHistorySubmitData = {
        id: this.integrationCallbackIncidentEventTypeChangeHistorySubmitData?.id || 0,
        integrationCallbackIncidentEventTypeId: Number(formValue.integrationCallbackIncidentEventTypeId),
        versionNumber: this.integrationCallbackIncidentEventTypeChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateIntegrationCallbackIncidentEventTypeChangeHistory(integrationCallbackIncidentEventTypeChangeHistorySubmitData);
      } else {
        this.addIntegrationCallbackIncidentEventTypeChangeHistory(integrationCallbackIncidentEventTypeChangeHistorySubmitData);
      }
  }

  private addIntegrationCallbackIncidentEventTypeChangeHistory(integrationCallbackIncidentEventTypeChangeHistoryData: IntegrationCallbackIncidentEventTypeChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    integrationCallbackIncidentEventTypeChangeHistoryData.versionNumber = 0;
    this.integrationCallbackIncidentEventTypeChangeHistoryService.PostIntegrationCallbackIncidentEventTypeChangeHistory(integrationCallbackIncidentEventTypeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIntegrationCallbackIncidentEventTypeChangeHistory) => {

        this.integrationCallbackIncidentEventTypeChangeHistoryService.ClearAllCaches();

        this.integrationCallbackIncidentEventTypeChangeHistoryChanged.next([newIntegrationCallbackIncidentEventTypeChangeHistory]);

        this.alertService.showMessage("Integration Callback Incident Event Type Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/integrationcallbackincidenteventtypechangehistory', newIntegrationCallbackIncidentEventTypeChangeHistory.id]);
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
                                   'You do not have permission to save this Integration Callback Incident Event Type Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Integration Callback Incident Event Type Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Integration Callback Incident Event Type Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIntegrationCallbackIncidentEventTypeChangeHistory(integrationCallbackIncidentEventTypeChangeHistoryData: IntegrationCallbackIncidentEventTypeChangeHistorySubmitData) {
    this.integrationCallbackIncidentEventTypeChangeHistoryService.PutIntegrationCallbackIncidentEventTypeChangeHistory(integrationCallbackIncidentEventTypeChangeHistoryData.id, integrationCallbackIncidentEventTypeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIntegrationCallbackIncidentEventTypeChangeHistory) => {

        this.integrationCallbackIncidentEventTypeChangeHistoryService.ClearAllCaches();

        this.integrationCallbackIncidentEventTypeChangeHistoryChanged.next([updatedIntegrationCallbackIncidentEventTypeChangeHistory]);

        this.alertService.showMessage("Integration Callback Incident Event Type Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Integration Callback Incident Event Type Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Integration Callback Incident Event Type Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Integration Callback Incident Event Type Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(integrationCallbackIncidentEventTypeChangeHistoryData: IntegrationCallbackIncidentEventTypeChangeHistoryData | null) {

    if (integrationCallbackIncidentEventTypeChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.integrationCallbackIncidentEventTypeChangeHistoryForm.reset({
        integrationCallbackIncidentEventTypeId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.integrationCallbackIncidentEventTypeChangeHistoryForm.reset({
        integrationCallbackIncidentEventTypeId: integrationCallbackIncidentEventTypeChangeHistoryData.integrationCallbackIncidentEventTypeId,
        versionNumber: integrationCallbackIncidentEventTypeChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(integrationCallbackIncidentEventTypeChangeHistoryData.timeStamp) ?? '',
        userId: integrationCallbackIncidentEventTypeChangeHistoryData.userId?.toString() ?? '',
        data: integrationCallbackIncidentEventTypeChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.integrationCallbackIncidentEventTypeChangeHistoryForm.markAsPristine();
    this.integrationCallbackIncidentEventTypeChangeHistoryForm.markAsUntouched();
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


  public userIsAlertingIntegrationCallbackIncidentEventTypeChangeHistoryReader(): boolean {
    return this.integrationCallbackIncidentEventTypeChangeHistoryService.userIsAlertingIntegrationCallbackIncidentEventTypeChangeHistoryReader();
  }

  public userIsAlertingIntegrationCallbackIncidentEventTypeChangeHistoryWriter(): boolean {
    return this.integrationCallbackIncidentEventTypeChangeHistoryService.userIsAlertingIntegrationCallbackIncidentEventTypeChangeHistoryWriter();
  }
}
