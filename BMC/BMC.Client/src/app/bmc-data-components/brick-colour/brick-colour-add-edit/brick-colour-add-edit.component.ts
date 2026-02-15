/*
   GENERATED FORM FOR THE BRICKCOLOUR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickColour table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-colour-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickColourService, BrickColourData, BrickColourSubmitData } from '../../../bmc-data-services/brick-colour.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ColourFinishService } from '../../../bmc-data-services/colour-finish.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickColourFormValues {
  name: string,
  ldrawColourCode: string,     // Stored as string for form input, converted to number on submit.
  hexRgb: string | null,
  hexEdgeColour: string | null,
  alpha: string | null,     // Stored as string for form input, converted to number on submit.
  isTransparent: boolean,
  isMetallic: boolean,
  colourFinishId: number | bigint,       // For FK link number
  luminance: string | null,     // Stored as string for form input, converted to number on submit.
  legoColourId: string | null,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-colour-add-edit',
  templateUrl: './brick-colour-add-edit.component.html',
  styleUrls: ['./brick-colour-add-edit.component.scss']
})
export class BrickColourAddEditComponent {
  @ViewChild('brickColourModal') brickColourModal!: TemplateRef<any>;
  @Output() brickColourChanged = new Subject<BrickColourData[]>();
  @Input() brickColourSubmitData: BrickColourSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickColourFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickColourForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        ldrawColourCode: ['', Validators.required],
        hexRgb: [''],
        hexEdgeColour: [''],
        alpha: [''],
        isTransparent: [false],
        isMetallic: [false],
        colourFinishId: [null, Validators.required],
        luminance: [''],
        legoColourId: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickColours$ = this.brickColourService.GetBrickColourList();
  colourFinishes$ = this.colourFinishService.GetColourFinishList();

  constructor(
    private modalService: NgbModal,
    private brickColourService: BrickColourService,
    private colourFinishService: ColourFinishService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickColourData?: BrickColourData) {

    if (brickColourData != null) {

      if (!this.brickColourService.userIsBMCBrickColourReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Colours`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickColourSubmitData = this.brickColourService.ConvertToBrickColourSubmitData(brickColourData);
      this.isEditMode = true;
      this.objectGuid = brickColourData.objectGuid;

      this.buildFormValues(brickColourData);

    } else {

      if (!this.brickColourService.userIsBMCBrickColourWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Colours`,
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
        this.brickColourForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickColourForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickColourModal, {
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

    if (this.brickColourService.userIsBMCBrickColourWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Colours`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickColourForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickColourForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickColourForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickColourSubmitData: BrickColourSubmitData = {
        id: this.brickColourSubmitData?.id || 0,
        name: formValue.name!.trim(),
        ldrawColourCode: Number(formValue.ldrawColourCode),
        hexRgb: formValue.hexRgb?.trim() || null,
        hexEdgeColour: formValue.hexEdgeColour?.trim() || null,
        alpha: formValue.alpha ? Number(formValue.alpha) : null,
        isTransparent: !!formValue.isTransparent,
        isMetallic: !!formValue.isMetallic,
        colourFinishId: Number(formValue.colourFinishId),
        luminance: formValue.luminance ? Number(formValue.luminance) : null,
        legoColourId: formValue.legoColourId ? Number(formValue.legoColourId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickColour(brickColourSubmitData);
      } else {
        this.addBrickColour(brickColourSubmitData);
      }
  }

  private addBrickColour(brickColourData: BrickColourSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickColourData.active = true;
    brickColourData.deleted = false;
    this.brickColourService.PostBrickColour(brickColourData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickColour) => {

        this.brickColourService.ClearAllCaches();

        this.brickColourChanged.next([newBrickColour]);

        this.alertService.showMessage("Brick Colour added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickcolour', newBrickColour.id]);
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
                                   'You do not have permission to save this Brick Colour.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Colour.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Colour could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickColour(brickColourData: BrickColourSubmitData) {
    this.brickColourService.PutBrickColour(brickColourData.id, brickColourData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickColour) => {

        this.brickColourService.ClearAllCaches();

        this.brickColourChanged.next([updatedBrickColour]);

        this.alertService.showMessage("Brick Colour updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Colour.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Colour.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Colour could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickColourData: BrickColourData | null) {

    if (brickColourData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickColourForm.reset({
        name: '',
        ldrawColourCode: '',
        hexRgb: '',
        hexEdgeColour: '',
        alpha: '',
        isTransparent: false,
        isMetallic: false,
        colourFinishId: null,
        luminance: '',
        legoColourId: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickColourForm.reset({
        name: brickColourData.name ?? '',
        ldrawColourCode: brickColourData.ldrawColourCode?.toString() ?? '',
        hexRgb: brickColourData.hexRgb ?? '',
        hexEdgeColour: brickColourData.hexEdgeColour ?? '',
        alpha: brickColourData.alpha?.toString() ?? '',
        isTransparent: brickColourData.isTransparent ?? false,
        isMetallic: brickColourData.isMetallic ?? false,
        colourFinishId: brickColourData.colourFinishId,
        luminance: brickColourData.luminance?.toString() ?? '',
        legoColourId: brickColourData.legoColourId?.toString() ?? '',
        sequence: brickColourData.sequence?.toString() ?? '',
        active: brickColourData.active ?? true,
        deleted: brickColourData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickColourForm.markAsPristine();
    this.brickColourForm.markAsUntouched();
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


  public userIsBMCBrickColourReader(): boolean {
    return this.brickColourService.userIsBMCBrickColourReader();
  }

  public userIsBMCBrickColourWriter(): boolean {
    return this.brickColourService.userIsBMCBrickColourWriter();
  }
}
