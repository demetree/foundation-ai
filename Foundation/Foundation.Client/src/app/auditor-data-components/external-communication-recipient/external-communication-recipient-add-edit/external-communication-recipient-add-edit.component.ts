/*
   GENERATED FORM FOR THE EXTERNALCOMMUNICATIONRECIPIENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ExternalCommunicationRecipient table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to external-communication-recipient-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ExternalCommunicationRecipientService, ExternalCommunicationRecipientData, ExternalCommunicationRecipientSubmitData } from '../../../auditor-data-services/external-communication-recipient.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ExternalCommunicationService } from '../../../auditor-data-services/external-communication.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ExternalCommunicationRecipientFormValues {
  externalCommunicationId: number | bigint | null,       // For FK link number
  recipient: string | null,
  type: string | null,
};

@Component({
  selector: 'app-external-communication-recipient-add-edit',
  templateUrl: './external-communication-recipient-add-edit.component.html',
  styleUrls: ['./external-communication-recipient-add-edit.component.scss']
})
export class ExternalCommunicationRecipientAddEditComponent {
  @ViewChild('externalCommunicationRecipientModal') externalCommunicationRecipientModal!: TemplateRef<any>;
  @Output() externalCommunicationRecipientChanged = new Subject<ExternalCommunicationRecipientData[]>();
  @Input() externalCommunicationRecipientSubmitData: ExternalCommunicationRecipientSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ExternalCommunicationRecipientFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public externalCommunicationRecipientForm: FormGroup = this.fb.group({
        externalCommunicationId: [null],
        recipient: [''],
        type: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  externalCommunicationRecipients$ = this.externalCommunicationRecipientService.GetExternalCommunicationRecipientList();
  externalCommunications$ = this.externalCommunicationService.GetExternalCommunicationList();

  constructor(
    private modalService: NgbModal,
    private externalCommunicationRecipientService: ExternalCommunicationRecipientService,
    private externalCommunicationService: ExternalCommunicationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(externalCommunicationRecipientData?: ExternalCommunicationRecipientData) {

    if (externalCommunicationRecipientData != null) {

      if (!this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read External Communication Recipients`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.externalCommunicationRecipientSubmitData = this.externalCommunicationRecipientService.ConvertToExternalCommunicationRecipientSubmitData(externalCommunicationRecipientData);
      this.isEditMode = true;

      this.buildFormValues(externalCommunicationRecipientData);

    } else {

      if (!this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write External Communication Recipients`,
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
        this.externalCommunicationRecipientForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.externalCommunicationRecipientForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.externalCommunicationRecipientModal, {
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

    if (this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write External Communication Recipients`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.externalCommunicationRecipientForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.externalCommunicationRecipientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.externalCommunicationRecipientForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const externalCommunicationRecipientSubmitData: ExternalCommunicationRecipientSubmitData = {
        id: this.externalCommunicationRecipientSubmitData?.id || 0,
        externalCommunicationId: formValue.externalCommunicationId ? Number(formValue.externalCommunicationId) : null,
        recipient: formValue.recipient?.trim() || null,
        type: formValue.type?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateExternalCommunicationRecipient(externalCommunicationRecipientSubmitData);
      } else {
        this.addExternalCommunicationRecipient(externalCommunicationRecipientSubmitData);
      }
  }

  private addExternalCommunicationRecipient(externalCommunicationRecipientData: ExternalCommunicationRecipientSubmitData) {
    this.externalCommunicationRecipientService.PostExternalCommunicationRecipient(externalCommunicationRecipientData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newExternalCommunicationRecipient) => {

        this.externalCommunicationRecipientService.ClearAllCaches();

        this.externalCommunicationRecipientChanged.next([newExternalCommunicationRecipient]);

        this.alertService.showMessage("External Communication Recipient added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/externalcommunicationrecipient', newExternalCommunicationRecipient.id]);
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
                                   'You do not have permission to save this External Communication Recipient.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication Recipient.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication Recipient could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateExternalCommunicationRecipient(externalCommunicationRecipientData: ExternalCommunicationRecipientSubmitData) {
    this.externalCommunicationRecipientService.PutExternalCommunicationRecipient(externalCommunicationRecipientData.id, externalCommunicationRecipientData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedExternalCommunicationRecipient) => {

        this.externalCommunicationRecipientService.ClearAllCaches();

        this.externalCommunicationRecipientChanged.next([updatedExternalCommunicationRecipient]);

        this.alertService.showMessage("External Communication Recipient updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this External Communication Recipient.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication Recipient.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication Recipient could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(externalCommunicationRecipientData: ExternalCommunicationRecipientData | null) {

    if (externalCommunicationRecipientData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.externalCommunicationRecipientForm.reset({
        externalCommunicationId: null,
        recipient: '',
        type: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.externalCommunicationRecipientForm.reset({
        externalCommunicationId: externalCommunicationRecipientData.externalCommunicationId,
        recipient: externalCommunicationRecipientData.recipient ?? '',
        type: externalCommunicationRecipientData.type ?? '',
      }, { emitEvent: false});
    }

    this.externalCommunicationRecipientForm.markAsPristine();
    this.externalCommunicationRecipientForm.markAsUntouched();
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


  public userIsAuditorExternalCommunicationRecipientReader(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientReader();
  }

  public userIsAuditorExternalCommunicationRecipientWriter(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter();
  }
}
