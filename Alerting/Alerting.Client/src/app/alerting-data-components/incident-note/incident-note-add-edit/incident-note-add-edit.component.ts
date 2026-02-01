/*
   GENERATED FORM FOR THE INCIDENTNOTE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentNote table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-note-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentNoteService, IncidentNoteData, IncidentNoteSubmitData } from '../../../alerting-data-services/incident-note.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IncidentService } from '../../../alerting-data-services/incident.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IncidentNoteFormValues {
  incidentId: number | bigint,       // For FK link number
  authorObjectGuid: string,
  createdAt: string,
  content: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-incident-note-add-edit',
  templateUrl: './incident-note-add-edit.component.html',
  styleUrls: ['./incident-note-add-edit.component.scss']
})
export class IncidentNoteAddEditComponent {
  @ViewChild('incidentNoteModal') incidentNoteModal!: TemplateRef<any>;
  @Output() incidentNoteChanged = new Subject<IncidentNoteData[]>();
  @Input() incidentNoteSubmitData: IncidentNoteSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentNoteFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentNoteForm: FormGroup = this.fb.group({
        incidentId: [null, Validators.required],
        authorObjectGuid: ['', Validators.required],
        createdAt: ['', Validators.required],
        content: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  incidentNotes$ = this.incidentNoteService.GetIncidentNoteList();
  incidents$ = this.incidentService.GetIncidentList();

  constructor(
    private modalService: NgbModal,
    private incidentNoteService: IncidentNoteService,
    private incidentService: IncidentService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(incidentNoteData?: IncidentNoteData) {

    if (incidentNoteData != null) {

      if (!this.incidentNoteService.userIsAlertingIncidentNoteReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Incident Notes`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.incidentNoteSubmitData = this.incidentNoteService.ConvertToIncidentNoteSubmitData(incidentNoteData);
      this.isEditMode = true;
      this.objectGuid = incidentNoteData.objectGuid;

      this.buildFormValues(incidentNoteData);

    } else {

      if (!this.incidentNoteService.userIsAlertingIncidentNoteWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Incident Notes`,
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
        this.incidentNoteForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentNoteForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.incidentNoteModal, {
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

    if (this.incidentNoteService.userIsAlertingIncidentNoteWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Incident Notes`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.incidentNoteForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentNoteForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentNoteForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentNoteSubmitData: IncidentNoteSubmitData = {
        id: this.incidentNoteSubmitData?.id || 0,
        incidentId: Number(formValue.incidentId),
        authorObjectGuid: formValue.authorObjectGuid!.trim(),
        createdAt: dateTimeLocalToIsoUtc(formValue.createdAt!.trim())!,
        content: formValue.content!.trim(),
        versionNumber: this.incidentNoteSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIncidentNote(incidentNoteSubmitData);
      } else {
        this.addIncidentNote(incidentNoteSubmitData);
      }
  }

  private addIncidentNote(incidentNoteData: IncidentNoteSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    incidentNoteData.versionNumber = 0;
    incidentNoteData.active = true;
    incidentNoteData.deleted = false;
    this.incidentNoteService.PostIncidentNote(incidentNoteData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIncidentNote) => {

        this.incidentNoteService.ClearAllCaches();

        this.incidentNoteChanged.next([newIncidentNote]);

        this.alertService.showMessage("Incident Note added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/incidentnote', newIncidentNote.id]);
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
                                   'You do not have permission to save this Incident Note.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Note.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Note could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIncidentNote(incidentNoteData: IncidentNoteSubmitData) {
    this.incidentNoteService.PutIncidentNote(incidentNoteData.id, incidentNoteData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIncidentNote) => {

        this.incidentNoteService.ClearAllCaches();

        this.incidentNoteChanged.next([updatedIncidentNote]);

        this.alertService.showMessage("Incident Note updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Incident Note.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Note.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Note could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(incidentNoteData: IncidentNoteData | null) {

    if (incidentNoteData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentNoteForm.reset({
        incidentId: null,
        authorObjectGuid: '',
        createdAt: '',
        content: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.incidentNoteForm.reset({
        incidentId: incidentNoteData.incidentId,
        authorObjectGuid: incidentNoteData.authorObjectGuid ?? '',
        createdAt: isoUtcStringToDateTimeLocal(incidentNoteData.createdAt) ?? '',
        content: incidentNoteData.content ?? '',
        versionNumber: incidentNoteData.versionNumber?.toString() ?? '',
        active: incidentNoteData.active ?? true,
        deleted: incidentNoteData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentNoteForm.markAsPristine();
    this.incidentNoteForm.markAsUntouched();
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


  public userIsAlertingIncidentNoteReader(): boolean {
    return this.incidentNoteService.userIsAlertingIncidentNoteReader();
  }

  public userIsAlertingIncidentNoteWriter(): boolean {
    return this.incidentNoteService.userIsAlertingIncidentNoteWriter();
  }
}
