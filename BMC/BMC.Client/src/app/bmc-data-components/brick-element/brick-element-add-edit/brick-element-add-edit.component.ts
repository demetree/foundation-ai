/*
   GENERATED FORM FOR THE BRICKELEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickElement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-element-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickElementService, BrickElementData, BrickElementSubmitData } from '../../../bmc-data-services/brick-element.service';
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
interface BrickElementFormValues {
  elementId: string,
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  designId: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-element-add-edit',
  templateUrl: './brick-element-add-edit.component.html',
  styleUrls: ['./brick-element-add-edit.component.scss']
})
export class BrickElementAddEditComponent {
  @ViewChild('brickElementModal') brickElementModal!: TemplateRef<any>;
  @Output() brickElementChanged = new Subject<BrickElementData[]>();
  @Input() brickElementSubmitData: BrickElementSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickElementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickElementForm: FormGroup = this.fb.group({
        elementId: ['', Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        designId: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickElements$ = this.brickElementService.GetBrickElementList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private brickElementService: BrickElementService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickElementData?: BrickElementData) {

    if (brickElementData != null) {

      if (!this.brickElementService.userIsBMCBrickElementReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Elements`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickElementSubmitData = this.brickElementService.ConvertToBrickElementSubmitData(brickElementData);
      this.isEditMode = true;
      this.objectGuid = brickElementData.objectGuid;

      this.buildFormValues(brickElementData);

    } else {

      if (!this.brickElementService.userIsBMCBrickElementWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Elements`,
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
        this.brickElementForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickElementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickElementModal, {
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

    if (this.brickElementService.userIsBMCBrickElementWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Elements`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickElementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickElementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickElementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickElementSubmitData: BrickElementSubmitData = {
        id: this.brickElementSubmitData?.id || 0,
        elementId: formValue.elementId!.trim(),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        designId: formValue.designId?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickElement(brickElementSubmitData);
      } else {
        this.addBrickElement(brickElementSubmitData);
      }
  }

  private addBrickElement(brickElementData: BrickElementSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickElementData.active = true;
    brickElementData.deleted = false;
    this.brickElementService.PostBrickElement(brickElementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickElement) => {

        this.brickElementService.ClearAllCaches();

        this.brickElementChanged.next([newBrickElement]);

        this.alertService.showMessage("Brick Element added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickelement', newBrickElement.id]);
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
                                   'You do not have permission to save this Brick Element.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Element.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Element could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickElement(brickElementData: BrickElementSubmitData) {
    this.brickElementService.PutBrickElement(brickElementData.id, brickElementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickElement) => {

        this.brickElementService.ClearAllCaches();

        this.brickElementChanged.next([updatedBrickElement]);

        this.alertService.showMessage("Brick Element updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Element.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Element.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Element could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickElementData: BrickElementData | null) {

    if (brickElementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickElementForm.reset({
        elementId: '',
        brickPartId: null,
        brickColourId: null,
        designId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickElementForm.reset({
        elementId: brickElementData.elementId ?? '',
        brickPartId: brickElementData.brickPartId,
        brickColourId: brickElementData.brickColourId,
        designId: brickElementData.designId ?? '',
        active: brickElementData.active ?? true,
        deleted: brickElementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickElementForm.markAsPristine();
    this.brickElementForm.markAsUntouched();
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


  public userIsBMCBrickElementReader(): boolean {
    return this.brickElementService.userIsBMCBrickElementReader();
  }

  public userIsBMCBrickElementWriter(): boolean {
    return this.brickElementService.userIsBMCBrickElementWriter();
  }
}
