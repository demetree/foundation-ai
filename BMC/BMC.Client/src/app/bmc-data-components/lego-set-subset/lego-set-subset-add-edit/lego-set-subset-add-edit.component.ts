/*
   GENERATED FORM FOR THE LEGOSETSUBSET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoSetSubset table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-set-subset-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoSetSubsetService, LegoSetSubsetData, LegoSetSubsetSubmitData } from '../../../bmc-data-services/lego-set-subset.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface LegoSetSubsetFormValues {
  parentLegoSetId: number | bigint,       // For FK link number
  childLegoSetId: number | bigint,       // For FK link number
  quantity: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-lego-set-subset-add-edit',
  templateUrl: './lego-set-subset-add-edit.component.html',
  styleUrls: ['./lego-set-subset-add-edit.component.scss']
})
export class LegoSetSubsetAddEditComponent {
  @ViewChild('legoSetSubsetModal') legoSetSubsetModal!: TemplateRef<any>;
  @Output() legoSetSubsetChanged = new Subject<LegoSetSubsetData[]>();
  @Input() legoSetSubsetSubmitData: LegoSetSubsetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoSetSubsetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoSetSubsetForm: FormGroup = this.fb.group({
        parentLegoSetId: [null, Validators.required],
        childLegoSetId: [null, Validators.required],
        quantity: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  legoSetSubsets$ = this.legoSetSubsetService.GetLegoSetSubsetList();
  legoSets$ = this.legoSetService.GetLegoSetList();

  constructor(
    private modalService: NgbModal,
    private legoSetSubsetService: LegoSetSubsetService,
    private legoSetService: LegoSetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(legoSetSubsetData?: LegoSetSubsetData) {

    if (legoSetSubsetData != null) {

      if (!this.legoSetSubsetService.userIsBMCLegoSetSubsetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Lego Set Subsets`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.legoSetSubsetSubmitData = this.legoSetSubsetService.ConvertToLegoSetSubsetSubmitData(legoSetSubsetData);
      this.isEditMode = true;
      this.objectGuid = legoSetSubsetData.objectGuid;

      this.buildFormValues(legoSetSubsetData);

    } else {

      if (!this.legoSetSubsetService.userIsBMCLegoSetSubsetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Lego Set Subsets`,
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
        this.legoSetSubsetForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoSetSubsetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.legoSetSubsetModal, {
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

    if (this.legoSetSubsetService.userIsBMCLegoSetSubsetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Lego Set Subsets`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.legoSetSubsetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoSetSubsetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoSetSubsetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoSetSubsetSubmitData: LegoSetSubsetSubmitData = {
        id: this.legoSetSubsetSubmitData?.id || 0,
        parentLegoSetId: Number(formValue.parentLegoSetId),
        childLegoSetId: Number(formValue.childLegoSetId),
        quantity: formValue.quantity ? Number(formValue.quantity) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateLegoSetSubset(legoSetSubsetSubmitData);
      } else {
        this.addLegoSetSubset(legoSetSubsetSubmitData);
      }
  }

  private addLegoSetSubset(legoSetSubsetData: LegoSetSubsetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    legoSetSubsetData.active = true;
    legoSetSubsetData.deleted = false;
    this.legoSetSubsetService.PostLegoSetSubset(legoSetSubsetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newLegoSetSubset) => {

        this.legoSetSubsetService.ClearAllCaches();

        this.legoSetSubsetChanged.next([newLegoSetSubset]);

        this.alertService.showMessage("Lego Set Subset added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/legosetsubset', newLegoSetSubset.id]);
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
                                   'You do not have permission to save this Lego Set Subset.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set Subset.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set Subset could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateLegoSetSubset(legoSetSubsetData: LegoSetSubsetSubmitData) {
    this.legoSetSubsetService.PutLegoSetSubset(legoSetSubsetData.id, legoSetSubsetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedLegoSetSubset) => {

        this.legoSetSubsetService.ClearAllCaches();

        this.legoSetSubsetChanged.next([updatedLegoSetSubset]);

        this.alertService.showMessage("Lego Set Subset updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Lego Set Subset.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set Subset.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set Subset could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(legoSetSubsetData: LegoSetSubsetData | null) {

    if (legoSetSubsetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoSetSubsetForm.reset({
        parentLegoSetId: null,
        childLegoSetId: null,
        quantity: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoSetSubsetForm.reset({
        parentLegoSetId: legoSetSubsetData.parentLegoSetId,
        childLegoSetId: legoSetSubsetData.childLegoSetId,
        quantity: legoSetSubsetData.quantity?.toString() ?? '',
        active: legoSetSubsetData.active ?? true,
        deleted: legoSetSubsetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoSetSubsetForm.markAsPristine();
    this.legoSetSubsetForm.markAsUntouched();
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


  public userIsBMCLegoSetSubsetReader(): boolean {
    return this.legoSetSubsetService.userIsBMCLegoSetSubsetReader();
  }

  public userIsBMCLegoSetSubsetWriter(): boolean {
    return this.legoSetSubsetService.userIsBMCLegoSetSubsetWriter();
  }
}
