/*
   GENERATED FORM FOR THE MOCCOLLABORATOR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocCollaborator table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-collaborator-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocCollaboratorService, MocCollaboratorData, MocCollaboratorSubmitData } from '../../../bmc-data-services/moc-collaborator.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MocCollaboratorFormValues {
  publishedMocId: number | bigint,       // For FK link number
  collaboratorTenantGuid: string,
  accessLevel: string,
  invitedDate: string,
  acceptedDate: string | null,
  isAccepted: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-moc-collaborator-add-edit',
  templateUrl: './moc-collaborator-add-edit.component.html',
  styleUrls: ['./moc-collaborator-add-edit.component.scss']
})
export class MocCollaboratorAddEditComponent {
  @ViewChild('mocCollaboratorModal') mocCollaboratorModal!: TemplateRef<any>;
  @Output() mocCollaboratorChanged = new Subject<MocCollaboratorData[]>();
  @Input() mocCollaboratorSubmitData: MocCollaboratorSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocCollaboratorFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocCollaboratorForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        collaboratorTenantGuid: ['', Validators.required],
        accessLevel: ['', Validators.required],
        invitedDate: ['', Validators.required],
        acceptedDate: [''],
        isAccepted: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  mocCollaborators$ = this.mocCollaboratorService.GetMocCollaboratorList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  constructor(
    private modalService: NgbModal,
    private mocCollaboratorService: MocCollaboratorService,
    private publishedMocService: PublishedMocService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(mocCollaboratorData?: MocCollaboratorData) {

    if (mocCollaboratorData != null) {

      if (!this.mocCollaboratorService.userIsBMCMocCollaboratorReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Moc Collaborators`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.mocCollaboratorSubmitData = this.mocCollaboratorService.ConvertToMocCollaboratorSubmitData(mocCollaboratorData);
      this.isEditMode = true;
      this.objectGuid = mocCollaboratorData.objectGuid;

      this.buildFormValues(mocCollaboratorData);

    } else {

      if (!this.mocCollaboratorService.userIsBMCMocCollaboratorWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Moc Collaborators`,
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
        this.mocCollaboratorForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocCollaboratorForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.mocCollaboratorModal, {
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

    if (this.mocCollaboratorService.userIsBMCMocCollaboratorWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Moc Collaborators`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.mocCollaboratorForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocCollaboratorForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocCollaboratorForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocCollaboratorSubmitData: MocCollaboratorSubmitData = {
        id: this.mocCollaboratorSubmitData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        collaboratorTenantGuid: formValue.collaboratorTenantGuid!.trim(),
        accessLevel: formValue.accessLevel!.trim(),
        invitedDate: dateTimeLocalToIsoUtc(formValue.invitedDate!.trim())!,
        acceptedDate: formValue.acceptedDate ? dateTimeLocalToIsoUtc(formValue.acceptedDate.trim()) : null,
        isAccepted: !!formValue.isAccepted,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMocCollaborator(mocCollaboratorSubmitData);
      } else {
        this.addMocCollaborator(mocCollaboratorSubmitData);
      }
  }

  private addMocCollaborator(mocCollaboratorData: MocCollaboratorSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    mocCollaboratorData.active = true;
    mocCollaboratorData.deleted = false;
    this.mocCollaboratorService.PostMocCollaborator(mocCollaboratorData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMocCollaborator) => {

        this.mocCollaboratorService.ClearAllCaches();

        this.mocCollaboratorChanged.next([newMocCollaborator]);

        this.alertService.showMessage("Moc Collaborator added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/moccollaborator', newMocCollaborator.id]);
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
                                   'You do not have permission to save this Moc Collaborator.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Collaborator.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Collaborator could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMocCollaborator(mocCollaboratorData: MocCollaboratorSubmitData) {
    this.mocCollaboratorService.PutMocCollaborator(mocCollaboratorData.id, mocCollaboratorData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMocCollaborator) => {

        this.mocCollaboratorService.ClearAllCaches();

        this.mocCollaboratorChanged.next([updatedMocCollaborator]);

        this.alertService.showMessage("Moc Collaborator updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Moc Collaborator.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Collaborator.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Collaborator could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(mocCollaboratorData: MocCollaboratorData | null) {

    if (mocCollaboratorData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocCollaboratorForm.reset({
        publishedMocId: null,
        collaboratorTenantGuid: '',
        accessLevel: '',
        invitedDate: '',
        acceptedDate: '',
        isAccepted: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocCollaboratorForm.reset({
        publishedMocId: mocCollaboratorData.publishedMocId,
        collaboratorTenantGuid: mocCollaboratorData.collaboratorTenantGuid ?? '',
        accessLevel: mocCollaboratorData.accessLevel ?? '',
        invitedDate: isoUtcStringToDateTimeLocal(mocCollaboratorData.invitedDate) ?? '',
        acceptedDate: isoUtcStringToDateTimeLocal(mocCollaboratorData.acceptedDate) ?? '',
        isAccepted: mocCollaboratorData.isAccepted ?? false,
        active: mocCollaboratorData.active ?? true,
        deleted: mocCollaboratorData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocCollaboratorForm.markAsPristine();
    this.mocCollaboratorForm.markAsUntouched();
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


  public userIsBMCMocCollaboratorReader(): boolean {
    return this.mocCollaboratorService.userIsBMCMocCollaboratorReader();
  }

  public userIsBMCMocCollaboratorWriter(): boolean {
    return this.mocCollaboratorService.userIsBMCMocCollaboratorWriter();
  }
}
