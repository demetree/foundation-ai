/*
   GENERATED FORM FOR THE INCIDENTEVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentEventType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-event-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentEventTypeService, IncidentEventTypeData, IncidentEventTypeSubmitData } from '../../../alerting-data-services/incident-event-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IncidentEventTypeFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-incident-event-type-add-edit',
  templateUrl: './incident-event-type-add-edit.component.html',
  styleUrls: ['./incident-event-type-add-edit.component.scss']
})
export class IncidentEventTypeAddEditComponent {
  @ViewChild('incidentEventTypeModal') incidentEventTypeModal!: TemplateRef<any>;
  @Output() incidentEventTypeChanged = new Subject<IncidentEventTypeData[]>();
  @Input() incidentEventTypeSubmitData: IncidentEventTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentEventTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentEventTypeForm: FormGroup = this.fb.group({
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

  incidentEventTypes$ = this.incidentEventTypeService.GetIncidentEventTypeList();

  constructor(
    private modalService: NgbModal,
    private incidentEventTypeService: IncidentEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(incidentEventTypeData?: IncidentEventTypeData) {

    if (incidentEventTypeData != null) {

      if (!this.incidentEventTypeService.userIsAlertingIncidentEventTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Incident Event Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.incidentEventTypeSubmitData = this.incidentEventTypeService.ConvertToIncidentEventTypeSubmitData(incidentEventTypeData);
      this.isEditMode = true;

      this.buildFormValues(incidentEventTypeData);

    } else {

      if (!this.incidentEventTypeService.userIsAlertingIncidentEventTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Incident Event Types`,
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
        this.incidentEventTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentEventTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.incidentEventTypeModal, {
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

    if (this.incidentEventTypeService.userIsAlertingIncidentEventTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Incident Event Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.incidentEventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentEventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentEventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentEventTypeSubmitData: IncidentEventTypeSubmitData = {
        id: this.incidentEventTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIncidentEventType(incidentEventTypeSubmitData);
      } else {
        this.addIncidentEventType(incidentEventTypeSubmitData);
      }
  }

  private addIncidentEventType(incidentEventTypeData: IncidentEventTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    incidentEventTypeData.active = true;
    incidentEventTypeData.deleted = false;
    this.incidentEventTypeService.PostIncidentEventType(incidentEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIncidentEventType) => {

        this.incidentEventTypeService.ClearAllCaches();

        this.incidentEventTypeChanged.next([newIncidentEventType]);

        this.alertService.showMessage("Incident Event Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/incidenteventtype', newIncidentEventType.id]);
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
                                   'You do not have permission to save this Incident Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIncidentEventType(incidentEventTypeData: IncidentEventTypeSubmitData) {
    this.incidentEventTypeService.PutIncidentEventType(incidentEventTypeData.id, incidentEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIncidentEventType) => {

        this.incidentEventTypeService.ClearAllCaches();

        this.incidentEventTypeChanged.next([updatedIncidentEventType]);

        this.alertService.showMessage("Incident Event Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Incident Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(incidentEventTypeData: IncidentEventTypeData | null) {

    if (incidentEventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentEventTypeForm.reset({
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
        this.incidentEventTypeForm.reset({
        name: incidentEventTypeData.name ?? '',
        description: incidentEventTypeData.description ?? '',
        active: incidentEventTypeData.active ?? true,
        deleted: incidentEventTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentEventTypeForm.markAsPristine();
    this.incidentEventTypeForm.markAsUntouched();
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


  public userIsAlertingIncidentEventTypeReader(): boolean {
    return this.incidentEventTypeService.userIsAlertingIncidentEventTypeReader();
  }

  public userIsAlertingIncidentEventTypeWriter(): boolean {
    return this.incidentEventTypeService.userIsAlertingIncidentEventTypeWriter();
  }
}
