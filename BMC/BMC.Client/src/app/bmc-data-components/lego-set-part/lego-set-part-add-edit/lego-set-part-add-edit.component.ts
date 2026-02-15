/*
   GENERATED FORM FOR THE LEGOSETPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoSetPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-set-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoSetPartService, LegoSetPartData, LegoSetPartSubmitData } from '../../../bmc-data-services/lego-set-part.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
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
interface LegoSetPartFormValues {
  legoSetId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  quantity: string | null,     // Stored as string for form input, converted to number on submit.
  isSpare: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-lego-set-part-add-edit',
  templateUrl: './lego-set-part-add-edit.component.html',
  styleUrls: ['./lego-set-part-add-edit.component.scss']
})
export class LegoSetPartAddEditComponent {
  @ViewChild('legoSetPartModal') legoSetPartModal!: TemplateRef<any>;
  @Output() legoSetPartChanged = new Subject<LegoSetPartData[]>();
  @Input() legoSetPartSubmitData: LegoSetPartSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoSetPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoSetPartForm: FormGroup = this.fb.group({
        legoSetId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        quantity: [''],
        isSpare: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  legoSetParts$ = this.legoSetPartService.GetLegoSetPartList();
  legoSets$ = this.legoSetService.GetLegoSetList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private legoSetPartService: LegoSetPartService,
    private legoSetService: LegoSetService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(legoSetPartData?: LegoSetPartData) {

    if (legoSetPartData != null) {

      if (!this.legoSetPartService.userIsBMCLegoSetPartReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Lego Set Parts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.legoSetPartSubmitData = this.legoSetPartService.ConvertToLegoSetPartSubmitData(legoSetPartData);
      this.isEditMode = true;
      this.objectGuid = legoSetPartData.objectGuid;

      this.buildFormValues(legoSetPartData);

    } else {

      if (!this.legoSetPartService.userIsBMCLegoSetPartWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Lego Set Parts`,
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
        this.legoSetPartForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoSetPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.legoSetPartModal, {
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

    if (this.legoSetPartService.userIsBMCLegoSetPartWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Lego Set Parts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.legoSetPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoSetPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoSetPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoSetPartSubmitData: LegoSetPartSubmitData = {
        id: this.legoSetPartSubmitData?.id || 0,
        legoSetId: Number(formValue.legoSetId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        quantity: formValue.quantity ? Number(formValue.quantity) : null,
        isSpare: !!formValue.isSpare,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateLegoSetPart(legoSetPartSubmitData);
      } else {
        this.addLegoSetPart(legoSetPartSubmitData);
      }
  }

  private addLegoSetPart(legoSetPartData: LegoSetPartSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    legoSetPartData.active = true;
    legoSetPartData.deleted = false;
    this.legoSetPartService.PostLegoSetPart(legoSetPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newLegoSetPart) => {

        this.legoSetPartService.ClearAllCaches();

        this.legoSetPartChanged.next([newLegoSetPart]);

        this.alertService.showMessage("Lego Set Part added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/legosetpart', newLegoSetPart.id]);
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
                                   'You do not have permission to save this Lego Set Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateLegoSetPart(legoSetPartData: LegoSetPartSubmitData) {
    this.legoSetPartService.PutLegoSetPart(legoSetPartData.id, legoSetPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedLegoSetPart) => {

        this.legoSetPartService.ClearAllCaches();

        this.legoSetPartChanged.next([updatedLegoSetPart]);

        this.alertService.showMessage("Lego Set Part updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Lego Set Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(legoSetPartData: LegoSetPartData | null) {

    if (legoSetPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoSetPartForm.reset({
        legoSetId: null,
        brickPartId: null,
        brickColourId: null,
        quantity: '',
        isSpare: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoSetPartForm.reset({
        legoSetId: legoSetPartData.legoSetId,
        brickPartId: legoSetPartData.brickPartId,
        brickColourId: legoSetPartData.brickColourId,
        quantity: legoSetPartData.quantity?.toString() ?? '',
        isSpare: legoSetPartData.isSpare ?? false,
        active: legoSetPartData.active ?? true,
        deleted: legoSetPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoSetPartForm.markAsPristine();
    this.legoSetPartForm.markAsUntouched();
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


  public userIsBMCLegoSetPartReader(): boolean {
    return this.legoSetPartService.userIsBMCLegoSetPartReader();
  }

  public userIsBMCLegoSetPartWriter(): boolean {
    return this.legoSetPartService.userIsBMCLegoSetPartWriter();
  }
}
