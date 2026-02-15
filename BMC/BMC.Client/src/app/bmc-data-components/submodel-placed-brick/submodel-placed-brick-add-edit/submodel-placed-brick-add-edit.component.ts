/*
   GENERATED FORM FOR THE SUBMODELPLACEDBRICK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SubmodelPlacedBrick table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-placed-brick-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelPlacedBrickService, SubmodelPlacedBrickData, SubmodelPlacedBrickSubmitData } from '../../../bmc-data-services/submodel-placed-brick.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SubmodelService } from '../../../bmc-data-services/submodel.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SubmodelPlacedBrickFormValues {
  submodelId: number | bigint,       // For FK link number
  placedBrickId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-submodel-placed-brick-add-edit',
  templateUrl: './submodel-placed-brick-add-edit.component.html',
  styleUrls: ['./submodel-placed-brick-add-edit.component.scss']
})
export class SubmodelPlacedBrickAddEditComponent {
  @ViewChild('submodelPlacedBrickModal') submodelPlacedBrickModal!: TemplateRef<any>;
  @Output() submodelPlacedBrickChanged = new Subject<SubmodelPlacedBrickData[]>();
  @Input() submodelPlacedBrickSubmitData: SubmodelPlacedBrickSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelPlacedBrickFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelPlacedBrickForm: FormGroup = this.fb.group({
        submodelId: [null, Validators.required],
        placedBrickId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  submodelPlacedBricks$ = this.submodelPlacedBrickService.GetSubmodelPlacedBrickList();
  submodels$ = this.submodelService.GetSubmodelList();
  placedBricks$ = this.placedBrickService.GetPlacedBrickList();

  constructor(
    private modalService: NgbModal,
    private submodelPlacedBrickService: SubmodelPlacedBrickService,
    private submodelService: SubmodelService,
    private placedBrickService: PlacedBrickService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(submodelPlacedBrickData?: SubmodelPlacedBrickData) {

    if (submodelPlacedBrickData != null) {

      if (!this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Submodel Placed Bricks`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.submodelPlacedBrickSubmitData = this.submodelPlacedBrickService.ConvertToSubmodelPlacedBrickSubmitData(submodelPlacedBrickData);
      this.isEditMode = true;
      this.objectGuid = submodelPlacedBrickData.objectGuid;

      this.buildFormValues(submodelPlacedBrickData);

    } else {

      if (!this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Submodel Placed Bricks`,
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
        this.submodelPlacedBrickForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelPlacedBrickForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.submodelPlacedBrickModal, {
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

    if (this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Submodel Placed Bricks`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.submodelPlacedBrickForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelPlacedBrickForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelPlacedBrickForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelPlacedBrickSubmitData: SubmodelPlacedBrickSubmitData = {
        id: this.submodelPlacedBrickSubmitData?.id || 0,
        submodelId: Number(formValue.submodelId),
        placedBrickId: Number(formValue.placedBrickId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSubmodelPlacedBrick(submodelPlacedBrickSubmitData);
      } else {
        this.addSubmodelPlacedBrick(submodelPlacedBrickSubmitData);
      }
  }

  private addSubmodelPlacedBrick(submodelPlacedBrickData: SubmodelPlacedBrickSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    submodelPlacedBrickData.active = true;
    submodelPlacedBrickData.deleted = false;
    this.submodelPlacedBrickService.PostSubmodelPlacedBrick(submodelPlacedBrickData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSubmodelPlacedBrick) => {

        this.submodelPlacedBrickService.ClearAllCaches();

        this.submodelPlacedBrickChanged.next([newSubmodelPlacedBrick]);

        this.alertService.showMessage("Submodel Placed Brick added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/submodelplacedbrick', newSubmodelPlacedBrick.id]);
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
                                   'You do not have permission to save this Submodel Placed Brick.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Placed Brick.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Placed Brick could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSubmodelPlacedBrick(submodelPlacedBrickData: SubmodelPlacedBrickSubmitData) {
    this.submodelPlacedBrickService.PutSubmodelPlacedBrick(submodelPlacedBrickData.id, submodelPlacedBrickData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSubmodelPlacedBrick) => {

        this.submodelPlacedBrickService.ClearAllCaches();

        this.submodelPlacedBrickChanged.next([updatedSubmodelPlacedBrick]);

        this.alertService.showMessage("Submodel Placed Brick updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Submodel Placed Brick.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Placed Brick.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Placed Brick could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(submodelPlacedBrickData: SubmodelPlacedBrickData | null) {

    if (submodelPlacedBrickData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelPlacedBrickForm.reset({
        submodelId: null,
        placedBrickId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.submodelPlacedBrickForm.reset({
        submodelId: submodelPlacedBrickData.submodelId,
        placedBrickId: submodelPlacedBrickData.placedBrickId,
        active: submodelPlacedBrickData.active ?? true,
        deleted: submodelPlacedBrickData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.submodelPlacedBrickForm.markAsPristine();
    this.submodelPlacedBrickForm.markAsUntouched();
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


  public userIsBMCSubmodelPlacedBrickReader(): boolean {
    return this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickReader();
  }

  public userIsBMCSubmodelPlacedBrickWriter(): boolean {
    return this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickWriter();
  }
}
