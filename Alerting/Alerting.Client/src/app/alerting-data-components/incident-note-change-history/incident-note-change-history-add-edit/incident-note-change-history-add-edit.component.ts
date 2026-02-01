/*
   GENERATED FORM FOR THE INCIDENTNOTECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentNoteChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-note-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentNoteChangeHistoryService, IncidentNoteChangeHistoryData, IncidentNoteChangeHistorySubmitData } from '../../../alerting-data-services/incident-note-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IncidentNoteService } from '../../../alerting-data-services/incident-note.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IncidentNoteChangeHistoryFormValues {
  incidentNoteId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-incident-note-change-history-add-edit',
  templateUrl: './incident-note-change-history-add-edit.component.html',
  styleUrls: ['./incident-note-change-history-add-edit.component.scss']
})
export class IncidentNoteChangeHistoryAddEditComponent {
  @ViewChild('incidentNoteChangeHistoryModal') incidentNoteChangeHistoryModal!: TemplateRef<any>;
  @Output() incidentNoteChangeHistoryChanged = new Subject<IncidentNoteChangeHistoryData[]>();
  @Input() incidentNoteChangeHistorySubmitData: IncidentNoteChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentNoteChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentNoteChangeHistoryForm: FormGroup = this.fb.group({
        incidentNoteId: [null, Validators.required],
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

  incidentNoteChangeHistories$ = this.incidentNoteChangeHistoryService.GetIncidentNoteChangeHistoryList();
  incidentNotes$ = this.incidentNoteService.GetIncidentNoteList();

  constructor(
    private modalService: NgbModal,
    private incidentNoteChangeHistoryService: IncidentNoteChangeHistoryService,
    private incidentNoteService: IncidentNoteService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(incidentNoteChangeHistoryData?: IncidentNoteChangeHistoryData) {

    if (incidentNoteChangeHistoryData != null) {

      if (!this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Incident Note Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.incidentNoteChangeHistorySubmitData = this.incidentNoteChangeHistoryService.ConvertToIncidentNoteChangeHistorySubmitData(incidentNoteChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(incidentNoteChangeHistoryData);

    } else {

      if (!this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Incident Note Change Histories`,
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
        this.incidentNoteChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentNoteChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.incidentNoteChangeHistoryModal, {
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

    if (this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Incident Note Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.incidentNoteChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentNoteChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentNoteChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentNoteChangeHistorySubmitData: IncidentNoteChangeHistorySubmitData = {
        id: this.incidentNoteChangeHistorySubmitData?.id || 0,
        incidentNoteId: Number(formValue.incidentNoteId),
        versionNumber: this.incidentNoteChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateIncidentNoteChangeHistory(incidentNoteChangeHistorySubmitData);
      } else {
        this.addIncidentNoteChangeHistory(incidentNoteChangeHistorySubmitData);
      }
  }

  private addIncidentNoteChangeHistory(incidentNoteChangeHistoryData: IncidentNoteChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    incidentNoteChangeHistoryData.versionNumber = 0;
    this.incidentNoteChangeHistoryService.PostIncidentNoteChangeHistory(incidentNoteChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIncidentNoteChangeHistory) => {

        this.incidentNoteChangeHistoryService.ClearAllCaches();

        this.incidentNoteChangeHistoryChanged.next([newIncidentNoteChangeHistory]);

        this.alertService.showMessage("Incident Note Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/incidentnotechangehistory', newIncidentNoteChangeHistory.id]);
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
                                   'You do not have permission to save this Incident Note Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Note Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Note Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIncidentNoteChangeHistory(incidentNoteChangeHistoryData: IncidentNoteChangeHistorySubmitData) {
    this.incidentNoteChangeHistoryService.PutIncidentNoteChangeHistory(incidentNoteChangeHistoryData.id, incidentNoteChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIncidentNoteChangeHistory) => {

        this.incidentNoteChangeHistoryService.ClearAllCaches();

        this.incidentNoteChangeHistoryChanged.next([updatedIncidentNoteChangeHistory]);

        this.alertService.showMessage("Incident Note Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Incident Note Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Note Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Note Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(incidentNoteChangeHistoryData: IncidentNoteChangeHistoryData | null) {

    if (incidentNoteChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentNoteChangeHistoryForm.reset({
        incidentNoteId: null,
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
        this.incidentNoteChangeHistoryForm.reset({
        incidentNoteId: incidentNoteChangeHistoryData.incidentNoteId,
        versionNumber: incidentNoteChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(incidentNoteChangeHistoryData.timeStamp) ?? '',
        userId: incidentNoteChangeHistoryData.userId?.toString() ?? '',
        data: incidentNoteChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.incidentNoteChangeHistoryForm.markAsPristine();
    this.incidentNoteChangeHistoryForm.markAsUntouched();
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


  public userIsAlertingIncidentNoteChangeHistoryReader(): boolean {
    return this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryReader();
  }

  public userIsAlertingIncidentNoteChangeHistoryWriter(): boolean {
    return this.incidentNoteChangeHistoryService.userIsAlertingIncidentNoteChangeHistoryWriter();
  }
}
