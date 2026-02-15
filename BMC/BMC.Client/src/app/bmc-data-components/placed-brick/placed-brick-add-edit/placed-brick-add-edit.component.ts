/*
   GENERATED FORM FOR THE PLACEDBRICK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PlacedBrick table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to placed-brick-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PlacedBrickService, PlacedBrickData, PlacedBrickSubmitData } from '../../../bmc-data-services/placed-brick.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ProjectService } from '../../../bmc-data-services/project.service';
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
interface PlacedBrickFormValues {
  projectId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationX: string | null,     // Stored as string for form input, converted to number on submit.
  rotationY: string | null,     // Stored as string for form input, converted to number on submit.
  rotationZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationW: string | null,     // Stored as string for form input, converted to number on submit.
  buildStepNumber: string | null,     // Stored as string for form input, converted to number on submit.
  isHidden: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-placed-brick-add-edit',
  templateUrl: './placed-brick-add-edit.component.html',
  styleUrls: ['./placed-brick-add-edit.component.scss']
})
export class PlacedBrickAddEditComponent {
  @ViewChild('placedBrickModal') placedBrickModal!: TemplateRef<any>;
  @Output() placedBrickChanged = new Subject<PlacedBrickData[]>();
  @Input() placedBrickSubmitData: PlacedBrickSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PlacedBrickFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public placedBrickForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        rotationX: [''],
        rotationY: [''],
        rotationZ: [''],
        rotationW: [''],
        buildStepNumber: [''],
        isHidden: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  placedBricks$ = this.placedBrickService.GetPlacedBrickList();
  projects$ = this.projectService.GetProjectList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private placedBrickService: PlacedBrickService,
    private projectService: ProjectService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(placedBrickData?: PlacedBrickData) {

    if (placedBrickData != null) {

      if (!this.placedBrickService.userIsBMCPlacedBrickReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Placed Bricks`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.placedBrickSubmitData = this.placedBrickService.ConvertToPlacedBrickSubmitData(placedBrickData);
      this.isEditMode = true;
      this.objectGuid = placedBrickData.objectGuid;

      this.buildFormValues(placedBrickData);

    } else {

      if (!this.placedBrickService.userIsBMCPlacedBrickWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Placed Bricks`,
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
        this.placedBrickForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.placedBrickForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.placedBrickModal, {
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

    if (this.placedBrickService.userIsBMCPlacedBrickWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Placed Bricks`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.placedBrickForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.placedBrickForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.placedBrickForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const placedBrickSubmitData: PlacedBrickSubmitData = {
        id: this.placedBrickSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        rotationX: formValue.rotationX ? Number(formValue.rotationX) : null,
        rotationY: formValue.rotationY ? Number(formValue.rotationY) : null,
        rotationZ: formValue.rotationZ ? Number(formValue.rotationZ) : null,
        rotationW: formValue.rotationW ? Number(formValue.rotationW) : null,
        buildStepNumber: formValue.buildStepNumber ? Number(formValue.buildStepNumber) : null,
        isHidden: !!formValue.isHidden,
        versionNumber: this.placedBrickSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePlacedBrick(placedBrickSubmitData);
      } else {
        this.addPlacedBrick(placedBrickSubmitData);
      }
  }

  private addPlacedBrick(placedBrickData: PlacedBrickSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    placedBrickData.versionNumber = 0;
    placedBrickData.active = true;
    placedBrickData.deleted = false;
    this.placedBrickService.PostPlacedBrick(placedBrickData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPlacedBrick) => {

        this.placedBrickService.ClearAllCaches();

        this.placedBrickChanged.next([newPlacedBrick]);

        this.alertService.showMessage("Placed Brick added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/placedbrick', newPlacedBrick.id]);
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
                                   'You do not have permission to save this Placed Brick.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Placed Brick.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Placed Brick could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePlacedBrick(placedBrickData: PlacedBrickSubmitData) {
    this.placedBrickService.PutPlacedBrick(placedBrickData.id, placedBrickData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPlacedBrick) => {

        this.placedBrickService.ClearAllCaches();

        this.placedBrickChanged.next([updatedPlacedBrick]);

        this.alertService.showMessage("Placed Brick updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Placed Brick.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Placed Brick.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Placed Brick could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(placedBrickData: PlacedBrickData | null) {

    if (placedBrickData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.placedBrickForm.reset({
        projectId: null,
        brickPartId: null,
        brickColourId: null,
        positionX: '',
        positionY: '',
        positionZ: '',
        rotationX: '',
        rotationY: '',
        rotationZ: '',
        rotationW: '',
        buildStepNumber: '',
        isHidden: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.placedBrickForm.reset({
        projectId: placedBrickData.projectId,
        brickPartId: placedBrickData.brickPartId,
        brickColourId: placedBrickData.brickColourId,
        positionX: placedBrickData.positionX?.toString() ?? '',
        positionY: placedBrickData.positionY?.toString() ?? '',
        positionZ: placedBrickData.positionZ?.toString() ?? '',
        rotationX: placedBrickData.rotationX?.toString() ?? '',
        rotationY: placedBrickData.rotationY?.toString() ?? '',
        rotationZ: placedBrickData.rotationZ?.toString() ?? '',
        rotationW: placedBrickData.rotationW?.toString() ?? '',
        buildStepNumber: placedBrickData.buildStepNumber?.toString() ?? '',
        isHidden: placedBrickData.isHidden ?? false,
        versionNumber: placedBrickData.versionNumber?.toString() ?? '',
        active: placedBrickData.active ?? true,
        deleted: placedBrickData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.placedBrickForm.markAsPristine();
    this.placedBrickForm.markAsUntouched();
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


  public userIsBMCPlacedBrickReader(): boolean {
    return this.placedBrickService.userIsBMCPlacedBrickReader();
  }

  public userIsBMCPlacedBrickWriter(): boolean {
    return this.placedBrickService.userIsBMCPlacedBrickWriter();
  }
}
