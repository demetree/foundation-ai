/*
   GENERATED FORM FOR THE COLOURFINISH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ColourFinish table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to colour-finish-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ColourFinishService, ColourFinishData, ColourFinishSubmitData } from '../../../bmc-data-services/colour-finish.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ColourFinishFormValues {
  name: string,
  description: string,
  requiresEnvironmentMap: boolean,
  isMatte: boolean,
  defaultAlpha: string | null,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-colour-finish-add-edit',
  templateUrl: './colour-finish-add-edit.component.html',
  styleUrls: ['./colour-finish-add-edit.component.scss']
})
export class ColourFinishAddEditComponent {
  @ViewChild('colourFinishModal') colourFinishModal!: TemplateRef<any>;
  @Output() colourFinishChanged = new Subject<ColourFinishData[]>();
  @Input() colourFinishSubmitData: ColourFinishSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ColourFinishFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public colourFinishForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        requiresEnvironmentMap: [false],
        isMatte: [false],
        defaultAlpha: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  colourFinishs$ = this.colourFinishService.GetColourFinishList();

  constructor(
    private modalService: NgbModal,
    private colourFinishService: ColourFinishService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(colourFinishData?: ColourFinishData) {

    if (colourFinishData != null) {

      if (!this.colourFinishService.userIsBMCColourFinishReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Colour Finishs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.colourFinishSubmitData = this.colourFinishService.ConvertToColourFinishSubmitData(colourFinishData);
      this.isEditMode = true;
      this.objectGuid = colourFinishData.objectGuid;

      this.buildFormValues(colourFinishData);

    } else {

      if (!this.colourFinishService.userIsBMCColourFinishWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Colour Finishs`,
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
        this.colourFinishForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.colourFinishForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.colourFinishModal, {
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

    if (this.colourFinishService.userIsBMCColourFinishWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Colour Finishs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.colourFinishForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.colourFinishForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.colourFinishForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const colourFinishSubmitData: ColourFinishSubmitData = {
        id: this.colourFinishSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        requiresEnvironmentMap: !!formValue.requiresEnvironmentMap,
        isMatte: !!formValue.isMatte,
        defaultAlpha: formValue.defaultAlpha ? Number(formValue.defaultAlpha) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateColourFinish(colourFinishSubmitData);
      } else {
        this.addColourFinish(colourFinishSubmitData);
      }
  }

  private addColourFinish(colourFinishData: ColourFinishSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    colourFinishData.active = true;
    colourFinishData.deleted = false;
    this.colourFinishService.PostColourFinish(colourFinishData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newColourFinish) => {

        this.colourFinishService.ClearAllCaches();

        this.colourFinishChanged.next([newColourFinish]);

        this.alertService.showMessage("Colour Finish added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/colourfinish', newColourFinish.id]);
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
                                   'You do not have permission to save this Colour Finish.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Colour Finish.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Colour Finish could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateColourFinish(colourFinishData: ColourFinishSubmitData) {
    this.colourFinishService.PutColourFinish(colourFinishData.id, colourFinishData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedColourFinish) => {

        this.colourFinishService.ClearAllCaches();

        this.colourFinishChanged.next([updatedColourFinish]);

        this.alertService.showMessage("Colour Finish updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Colour Finish.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Colour Finish.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Colour Finish could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(colourFinishData: ColourFinishData | null) {

    if (colourFinishData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.colourFinishForm.reset({
        name: '',
        description: '',
        requiresEnvironmentMap: false,
        isMatte: false,
        defaultAlpha: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.colourFinishForm.reset({
        name: colourFinishData.name ?? '',
        description: colourFinishData.description ?? '',
        requiresEnvironmentMap: colourFinishData.requiresEnvironmentMap ?? false,
        isMatte: colourFinishData.isMatte ?? false,
        defaultAlpha: colourFinishData.defaultAlpha?.toString() ?? '',
        sequence: colourFinishData.sequence?.toString() ?? '',
        active: colourFinishData.active ?? true,
        deleted: colourFinishData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.colourFinishForm.markAsPristine();
    this.colourFinishForm.markAsUntouched();
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


  public userIsBMCColourFinishReader(): boolean {
    return this.colourFinishService.userIsBMCColourFinishReader();
  }

  public userIsBMCColourFinishWriter(): boolean {
    return this.colourFinishService.userIsBMCColourFinishWriter();
  }
}
