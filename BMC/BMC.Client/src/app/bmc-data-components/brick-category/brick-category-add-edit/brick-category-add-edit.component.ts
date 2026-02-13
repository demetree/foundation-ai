/*
   GENERATED FORM FOR THE BRICKCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickCategory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-category-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickCategoryService, BrickCategoryData, BrickCategorySubmitData } from '../../../bmc-data-services/brick-category.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickCategoryFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-category-add-edit',
  templateUrl: './brick-category-add-edit.component.html',
  styleUrls: ['./brick-category-add-edit.component.scss']
})
export class BrickCategoryAddEditComponent {
  @ViewChild('brickCategoryModal') brickCategoryModal!: TemplateRef<any>;
  @Output() brickCategoryChanged = new Subject<BrickCategoryData[]>();
  @Input() brickCategorySubmitData: BrickCategorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickCategoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickCategoryForm: FormGroup = this.fb.group({
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

  brickCategories$ = this.brickCategoryService.GetBrickCategoryList();

  constructor(
    private modalService: NgbModal,
    private brickCategoryService: BrickCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickCategoryData?: BrickCategoryData) {

    if (brickCategoryData != null) {

      if (!this.brickCategoryService.userIsBMCBrickCategoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Categories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickCategorySubmitData = this.brickCategoryService.ConvertToBrickCategorySubmitData(brickCategoryData);
      this.isEditMode = true;
      this.objectGuid = brickCategoryData.objectGuid;

      this.buildFormValues(brickCategoryData);

    } else {

      if (!this.brickCategoryService.userIsBMCBrickCategoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Categories`,
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
        this.brickCategoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickCategoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickCategoryModal, {
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

    if (this.brickCategoryService.userIsBMCBrickCategoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Categories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickCategoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickCategoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickCategoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickCategorySubmitData: BrickCategorySubmitData = {
        id: this.brickCategorySubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickCategory(brickCategorySubmitData);
      } else {
        this.addBrickCategory(brickCategorySubmitData);
      }
  }

  private addBrickCategory(brickCategoryData: BrickCategorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickCategoryData.active = true;
    brickCategoryData.deleted = false;
    this.brickCategoryService.PostBrickCategory(brickCategoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickCategory) => {

        this.brickCategoryService.ClearAllCaches();

        this.brickCategoryChanged.next([newBrickCategory]);

        this.alertService.showMessage("Brick Category added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickcategory', newBrickCategory.id]);
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
                                   'You do not have permission to save this Brick Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickCategory(brickCategoryData: BrickCategorySubmitData) {
    this.brickCategoryService.PutBrickCategory(brickCategoryData.id, brickCategoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickCategory) => {

        this.brickCategoryService.ClearAllCaches();

        this.brickCategoryChanged.next([updatedBrickCategory]);

        this.alertService.showMessage("Brick Category updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickCategoryData: BrickCategoryData | null) {

    if (brickCategoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickCategoryForm.reset({
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
        this.brickCategoryForm.reset({
        name: brickCategoryData.name ?? '',
        description: brickCategoryData.description ?? '',
        sequence: brickCategoryData.sequence?.toString() ?? '',
        active: brickCategoryData.active ?? true,
        deleted: brickCategoryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickCategoryForm.markAsPristine();
    this.brickCategoryForm.markAsUntouched();
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


  public userIsBMCBrickCategoryReader(): boolean {
    return this.brickCategoryService.userIsBMCBrickCategoryReader();
  }

  public userIsBMCBrickCategoryWriter(): boolean {
    return this.brickCategoryService.userIsBMCBrickCategoryWriter();
  }
}
