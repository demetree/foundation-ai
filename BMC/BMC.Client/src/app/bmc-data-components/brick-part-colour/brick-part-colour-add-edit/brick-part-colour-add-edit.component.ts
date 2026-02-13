/*
   GENERATED FORM FOR THE BRICKPARTCOLOUR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPartColour table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-colour-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickPartColourService, BrickPartColourData, BrickPartColourSubmitData } from '../../../bmc-data-services/brick-part-colour.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickPartColourFormValues {
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-part-colour-add-edit',
  templateUrl: './brick-part-colour-add-edit.component.html',
  styleUrls: ['./brick-part-colour-add-edit.component.scss']
})
export class BrickPartColourAddEditComponent {
  @ViewChild('brickPartColourModal') brickPartColourModal!: TemplateRef<any>;
  @Output() brickPartColourChanged = new Subject<BrickPartColourData[]>();
  @Input() brickPartColourSubmitData: BrickPartColourSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickPartColourFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickPartColourForm: FormGroup = this.fb.group({
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickPartColours$ = this.brickPartColourService.GetBrickPartColourList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private brickPartColourService: BrickPartColourService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickPartColourData?: BrickPartColourData) {

    if (brickPartColourData != null) {

      if (!this.brickPartColourService.userIsBMCBrickPartColourReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Part Colours`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickPartColourSubmitData = this.brickPartColourService.ConvertToBrickPartColourSubmitData(brickPartColourData);
      this.isEditMode = true;
      this.objectGuid = brickPartColourData.objectGuid;

      this.buildFormValues(brickPartColourData);

    } else {

      if (!this.brickPartColourService.userIsBMCBrickPartColourWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Part Colours`,
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
        this.brickPartColourForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickPartColourForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickPartColourModal, {
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

    if (this.brickPartColourService.userIsBMCBrickPartColourWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Part Colours`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickPartColourForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickPartColourForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickPartColourForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickPartColourSubmitData: BrickPartColourSubmitData = {
        id: this.brickPartColourSubmitData?.id || 0,
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickPartColour(brickPartColourSubmitData);
      } else {
        this.addBrickPartColour(brickPartColourSubmitData);
      }
  }

  private addBrickPartColour(brickPartColourData: BrickPartColourSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickPartColourData.active = true;
    brickPartColourData.deleted = false;
    this.brickPartColourService.PostBrickPartColour(brickPartColourData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickPartColour) => {

        this.brickPartColourService.ClearAllCaches();

        this.brickPartColourChanged.next([newBrickPartColour]);

        this.alertService.showMessage("Brick Part Colour added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickpartcolour', newBrickPartColour.id]);
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
                                   'You do not have permission to save this Brick Part Colour.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part Colour.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part Colour could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickPartColour(brickPartColourData: BrickPartColourSubmitData) {
    this.brickPartColourService.PutBrickPartColour(brickPartColourData.id, brickPartColourData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickPartColour) => {

        this.brickPartColourService.ClearAllCaches();

        this.brickPartColourChanged.next([updatedBrickPartColour]);

        this.alertService.showMessage("Brick Part Colour updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Part Colour.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part Colour.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part Colour could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickPartColourData: BrickPartColourData | null) {

    if (brickPartColourData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickPartColourForm.reset({
        brickPartId: null,
        brickColourId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickPartColourForm.reset({
        brickPartId: brickPartColourData.brickPartId,
        brickColourId: brickPartColourData.brickColourId,
        active: brickPartColourData.active ?? true,
        deleted: brickPartColourData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickPartColourForm.markAsPristine();
    this.brickPartColourForm.markAsUntouched();
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


  public userIsBMCBrickPartColourReader(): boolean {
    return this.brickPartColourService.userIsBMCBrickPartColourReader();
  }

  public userIsBMCBrickPartColourWriter(): boolean {
    return this.brickPartColourService.userIsBMCBrickPartColourWriter();
  }
}
