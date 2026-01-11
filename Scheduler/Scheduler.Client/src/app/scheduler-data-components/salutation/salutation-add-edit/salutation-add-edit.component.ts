/*
   GENERATED FORM FOR THE SALUTATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Salutation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to salutation-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SalutationService, SalutationData, SalutationSubmitData } from '../../../scheduler-data-services/salutation.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SalutationFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-salutation-add-edit',
  templateUrl: './salutation-add-edit.component.html',
  styleUrls: ['./salutation-add-edit.component.scss']
})
export class SalutationAddEditComponent {
  @ViewChild('salutationModal') salutationModal!: TemplateRef<any>;
  @Output() salutationChanged = new Subject<SalutationData[]>();
  @Input() salutationSubmitData: SalutationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SalutationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public salutationForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  salutations$ = this.salutationService.GetSalutationList();

  constructor(
    private modalService: NgbModal,
    private salutationService: SalutationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(salutationData?: SalutationData) {

    if (salutationData != null) {

      if (!this.salutationService.userIsSchedulerSalutationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Salutations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.salutationSubmitData = this.salutationService.ConvertToSalutationSubmitData(salutationData);
      this.isEditMode = true;
      this.objectGuid = salutationData.objectGuid;

      this.buildFormValues(salutationData);

    } else {

      if (!this.salutationService.userIsSchedulerSalutationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Salutations`,
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
        this.salutationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.salutationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.salutationModal, {
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

    if (this.salutationService.userIsSchedulerSalutationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Salutations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.salutationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.salutationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.salutationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const salutationSubmitData: SalutationSubmitData = {
        id: this.salutationSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSalutation(salutationSubmitData);
      } else {
        this.addSalutation(salutationSubmitData);
      }
  }

  private addSalutation(salutationData: SalutationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    salutationData.active = true;
    salutationData.deleted = false;
    this.salutationService.PostSalutation(salutationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSalutation) => {

        this.salutationService.ClearAllCaches();

        this.salutationChanged.next([newSalutation]);

        this.alertService.showMessage("Salutation added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/salutation', newSalutation.id]);
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
                                   'You do not have permission to save this Salutation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Salutation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Salutation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSalutation(salutationData: SalutationSubmitData) {
    this.salutationService.PutSalutation(salutationData.id, salutationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSalutation) => {

        this.salutationService.ClearAllCaches();

        this.salutationChanged.next([updatedSalutation]);

        this.alertService.showMessage("Salutation updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Salutation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Salutation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Salutation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(salutationData: SalutationData | null) {

    if (salutationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.salutationForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.salutationForm.reset({
        name: salutationData.name ?? '',
        description: salutationData.description ?? '',
        sequence: salutationData.sequence?.toString() ?? '',
        active: salutationData.active ?? true,
        deleted: salutationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.salutationForm.markAsPristine();
    this.salutationForm.markAsUntouched();
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


  public userIsSchedulerSalutationReader(): boolean {
    return this.salutationService.userIsSchedulerSalutationReader();
  }

  public userIsSchedulerSalutationWriter(): boolean {
    return this.salutationService.userIsSchedulerSalutationWriter();
  }
}
