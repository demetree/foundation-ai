/*
   GENERATED FORM FOR THE LEGOSETMINIFIG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoSetMinifig table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-set-minifig-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoSetMinifigService, LegoSetMinifigData, LegoSetMinifigSubmitData } from '../../../bmc-data-services/lego-set-minifig.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
import { LegoMinifigService } from '../../../bmc-data-services/lego-minifig.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface LegoSetMinifigFormValues {
  legoSetId: number | bigint,       // For FK link number
  legoMinifigId: number | bigint,       // For FK link number
  quantity: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-lego-set-minifig-add-edit',
  templateUrl: './lego-set-minifig-add-edit.component.html',
  styleUrls: ['./lego-set-minifig-add-edit.component.scss']
})
export class LegoSetMinifigAddEditComponent {
  @ViewChild('legoSetMinifigModal') legoSetMinifigModal!: TemplateRef<any>;
  @Output() legoSetMinifigChanged = new Subject<LegoSetMinifigData[]>();
  @Input() legoSetMinifigSubmitData: LegoSetMinifigSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoSetMinifigFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoSetMinifigForm: FormGroup = this.fb.group({
        legoSetId: [null, Validators.required],
        legoMinifigId: [null, Validators.required],
        quantity: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  legoSetMinifigs$ = this.legoSetMinifigService.GetLegoSetMinifigList();
  legoSets$ = this.legoSetService.GetLegoSetList();
  legoMinifigs$ = this.legoMinifigService.GetLegoMinifigList();

  constructor(
    private modalService: NgbModal,
    private legoSetMinifigService: LegoSetMinifigService,
    private legoSetService: LegoSetService,
    private legoMinifigService: LegoMinifigService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(legoSetMinifigData?: LegoSetMinifigData) {

    if (legoSetMinifigData != null) {

      if (!this.legoSetMinifigService.userIsBMCLegoSetMinifigReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Lego Set Minifigs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.legoSetMinifigSubmitData = this.legoSetMinifigService.ConvertToLegoSetMinifigSubmitData(legoSetMinifigData);
      this.isEditMode = true;
      this.objectGuid = legoSetMinifigData.objectGuid;

      this.buildFormValues(legoSetMinifigData);

    } else {

      if (!this.legoSetMinifigService.userIsBMCLegoSetMinifigWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Lego Set Minifigs`,
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
        this.legoSetMinifigForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoSetMinifigForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.legoSetMinifigModal, {
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

    if (this.legoSetMinifigService.userIsBMCLegoSetMinifigWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Lego Set Minifigs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.legoSetMinifigForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoSetMinifigForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoSetMinifigForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoSetMinifigSubmitData: LegoSetMinifigSubmitData = {
        id: this.legoSetMinifigSubmitData?.id || 0,
        legoSetId: Number(formValue.legoSetId),
        legoMinifigId: Number(formValue.legoMinifigId),
        quantity: formValue.quantity ? Number(formValue.quantity) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateLegoSetMinifig(legoSetMinifigSubmitData);
      } else {
        this.addLegoSetMinifig(legoSetMinifigSubmitData);
      }
  }

  private addLegoSetMinifig(legoSetMinifigData: LegoSetMinifigSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    legoSetMinifigData.active = true;
    legoSetMinifigData.deleted = false;
    this.legoSetMinifigService.PostLegoSetMinifig(legoSetMinifigData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newLegoSetMinifig) => {

        this.legoSetMinifigService.ClearAllCaches();

        this.legoSetMinifigChanged.next([newLegoSetMinifig]);

        this.alertService.showMessage("Lego Set Minifig added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/legosetminifig', newLegoSetMinifig.id]);
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
                                   'You do not have permission to save this Lego Set Minifig.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set Minifig.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set Minifig could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateLegoSetMinifig(legoSetMinifigData: LegoSetMinifigSubmitData) {
    this.legoSetMinifigService.PutLegoSetMinifig(legoSetMinifigData.id, legoSetMinifigData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedLegoSetMinifig) => {

        this.legoSetMinifigService.ClearAllCaches();

        this.legoSetMinifigChanged.next([updatedLegoSetMinifig]);

        this.alertService.showMessage("Lego Set Minifig updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Lego Set Minifig.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set Minifig.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set Minifig could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(legoSetMinifigData: LegoSetMinifigData | null) {

    if (legoSetMinifigData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoSetMinifigForm.reset({
        legoSetId: null,
        legoMinifigId: null,
        quantity: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoSetMinifigForm.reset({
        legoSetId: legoSetMinifigData.legoSetId,
        legoMinifigId: legoSetMinifigData.legoMinifigId,
        quantity: legoSetMinifigData.quantity?.toString() ?? '',
        active: legoSetMinifigData.active ?? true,
        deleted: legoSetMinifigData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoSetMinifigForm.markAsPristine();
    this.legoSetMinifigForm.markAsUntouched();
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


  public userIsBMCLegoSetMinifigReader(): boolean {
    return this.legoSetMinifigService.userIsBMCLegoSetMinifigReader();
  }

  public userIsBMCLegoSetMinifigWriter(): boolean {
    return this.legoSetMinifigService.userIsBMCLegoSetMinifigWriter();
  }
}
