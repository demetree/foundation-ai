/*
   GENERATED FORM FOR THE LEGOMINIFIG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoMinifig table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-minifig-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoMinifigService, LegoMinifigData, LegoMinifigSubmitData } from '../../../bmc-data-services/lego-minifig.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface LegoMinifigFormValues {
  name: string,
  figNumber: string,
  partCount: string,     // Stored as string for form input, converted to number on submit.
  imageUrl: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-lego-minifig-add-edit',
  templateUrl: './lego-minifig-add-edit.component.html',
  styleUrls: ['./lego-minifig-add-edit.component.scss']
})
export class LegoMinifigAddEditComponent {
  @ViewChild('legoMinifigModal') legoMinifigModal!: TemplateRef<any>;
  @Output() legoMinifigChanged = new Subject<LegoMinifigData[]>();
  @Input() legoMinifigSubmitData: LegoMinifigSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoMinifigFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoMinifigForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        figNumber: ['', Validators.required],
        partCount: ['', Validators.required],
        imageUrl: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  legoMinifigs$ = this.legoMinifigService.GetLegoMinifigList();

  constructor(
    private modalService: NgbModal,
    private legoMinifigService: LegoMinifigService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(legoMinifigData?: LegoMinifigData) {

    if (legoMinifigData != null) {

      if (!this.legoMinifigService.userIsBMCLegoMinifigReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Lego Minifigs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.legoMinifigSubmitData = this.legoMinifigService.ConvertToLegoMinifigSubmitData(legoMinifigData);
      this.isEditMode = true;
      this.objectGuid = legoMinifigData.objectGuid;

      this.buildFormValues(legoMinifigData);

    } else {

      if (!this.legoMinifigService.userIsBMCLegoMinifigWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Lego Minifigs`,
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
        this.legoMinifigForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoMinifigForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.legoMinifigModal, {
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

    if (this.legoMinifigService.userIsBMCLegoMinifigWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Lego Minifigs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.legoMinifigForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoMinifigForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoMinifigForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoMinifigSubmitData: LegoMinifigSubmitData = {
        id: this.legoMinifigSubmitData?.id || 0,
        name: formValue.name!.trim(),
        figNumber: formValue.figNumber!.trim(),
        partCount: Number(formValue.partCount),
        imageUrl: formValue.imageUrl?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateLegoMinifig(legoMinifigSubmitData);
      } else {
        this.addLegoMinifig(legoMinifigSubmitData);
      }
  }

  private addLegoMinifig(legoMinifigData: LegoMinifigSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    legoMinifigData.active = true;
    legoMinifigData.deleted = false;
    this.legoMinifigService.PostLegoMinifig(legoMinifigData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newLegoMinifig) => {

        this.legoMinifigService.ClearAllCaches();

        this.legoMinifigChanged.next([newLegoMinifig]);

        this.alertService.showMessage("Lego Minifig added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/legominifig', newLegoMinifig.id]);
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
                                   'You do not have permission to save this Lego Minifig.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Minifig.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Minifig could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateLegoMinifig(legoMinifigData: LegoMinifigSubmitData) {
    this.legoMinifigService.PutLegoMinifig(legoMinifigData.id, legoMinifigData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedLegoMinifig) => {

        this.legoMinifigService.ClearAllCaches();

        this.legoMinifigChanged.next([updatedLegoMinifig]);

        this.alertService.showMessage("Lego Minifig updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Lego Minifig.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Minifig.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Minifig could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(legoMinifigData: LegoMinifigData | null) {

    if (legoMinifigData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoMinifigForm.reset({
        name: '',
        figNumber: '',
        partCount: '',
        imageUrl: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoMinifigForm.reset({
        name: legoMinifigData.name ?? '',
        figNumber: legoMinifigData.figNumber ?? '',
        partCount: legoMinifigData.partCount?.toString() ?? '',
        imageUrl: legoMinifigData.imageUrl ?? '',
        active: legoMinifigData.active ?? true,
        deleted: legoMinifigData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoMinifigForm.markAsPristine();
    this.legoMinifigForm.markAsUntouched();
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


  public userIsBMCLegoMinifigReader(): boolean {
    return this.legoMinifigService.userIsBMCLegoMinifigReader();
  }

  public userIsBMCLegoMinifigWriter(): boolean {
    return this.legoMinifigService.userIsBMCLegoMinifigWriter();
  }
}
