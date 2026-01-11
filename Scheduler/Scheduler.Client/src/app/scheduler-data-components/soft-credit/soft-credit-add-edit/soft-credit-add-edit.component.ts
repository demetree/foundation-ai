/*
   GENERATED FORM FOR THE SOFTCREDIT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SoftCredit table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to soft-credit-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SoftCreditService, SoftCreditData, SoftCreditSubmitData } from '../../../scheduler-data-services/soft-credit.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { GiftService } from '../../../scheduler-data-services/gift.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SoftCreditFormValues {
  giftId: number | bigint,       // For FK link number
  constituentId: number | bigint,       // For FK link number
  amount: string,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-soft-credit-add-edit',
  templateUrl: './soft-credit-add-edit.component.html',
  styleUrls: ['./soft-credit-add-edit.component.scss']
})
export class SoftCreditAddEditComponent {
  @ViewChild('softCreditModal') softCreditModal!: TemplateRef<any>;
  @Output() softCreditChanged = new Subject<SoftCreditData[]>();
  @Input() softCreditSubmitData: SoftCreditSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SoftCreditFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public softCreditForm: FormGroup = this.fb.group({
        giftId: [null, Validators.required],
        constituentId: [null, Validators.required],
        amount: ['', Validators.required],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  softCredits$ = this.softCreditService.GetSoftCreditList();
  gifts$ = this.giftService.GetGiftList();
  constituents$ = this.constituentService.GetConstituentList();

  constructor(
    private modalService: NgbModal,
    private softCreditService: SoftCreditService,
    private giftService: GiftService,
    private constituentService: ConstituentService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(softCreditData?: SoftCreditData) {

    if (softCreditData != null) {

      if (!this.softCreditService.userIsSchedulerSoftCreditReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Soft Credits`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.softCreditSubmitData = this.softCreditService.ConvertToSoftCreditSubmitData(softCreditData);
      this.isEditMode = true;
      this.objectGuid = softCreditData.objectGuid;

      this.buildFormValues(softCreditData);

    } else {

      if (!this.softCreditService.userIsSchedulerSoftCreditWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Soft Credits`,
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
        this.softCreditForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.softCreditForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.softCreditModal, {
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

    if (this.softCreditService.userIsSchedulerSoftCreditWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Soft Credits`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.softCreditForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.softCreditForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.softCreditForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const softCreditSubmitData: SoftCreditSubmitData = {
        id: this.softCreditSubmitData?.id || 0,
        giftId: Number(formValue.giftId),
        constituentId: Number(formValue.constituentId),
        amount: Number(formValue.amount),
        notes: formValue.notes?.trim() || null,
        versionNumber: this.softCreditSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSoftCredit(softCreditSubmitData);
      } else {
        this.addSoftCredit(softCreditSubmitData);
      }
  }

  private addSoftCredit(softCreditData: SoftCreditSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    softCreditData.versionNumber = 0;
    softCreditData.active = true;
    softCreditData.deleted = false;
    this.softCreditService.PostSoftCredit(softCreditData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSoftCredit) => {

        this.softCreditService.ClearAllCaches();

        this.softCreditChanged.next([newSoftCredit]);

        this.alertService.showMessage("Soft Credit added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/softcredit', newSoftCredit.id]);
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
                                   'You do not have permission to save this Soft Credit.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Soft Credit.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Soft Credit could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSoftCredit(softCreditData: SoftCreditSubmitData) {
    this.softCreditService.PutSoftCredit(softCreditData.id, softCreditData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSoftCredit) => {

        this.softCreditService.ClearAllCaches();

        this.softCreditChanged.next([updatedSoftCredit]);

        this.alertService.showMessage("Soft Credit updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Soft Credit.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Soft Credit.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Soft Credit could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(softCreditData: SoftCreditData | null) {

    if (softCreditData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.softCreditForm.reset({
        giftId: null,
        constituentId: null,
        amount: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.softCreditForm.reset({
        giftId: softCreditData.giftId,
        constituentId: softCreditData.constituentId,
        amount: softCreditData.amount?.toString() ?? '',
        notes: softCreditData.notes ?? '',
        versionNumber: softCreditData.versionNumber?.toString() ?? '',
        active: softCreditData.active ?? true,
        deleted: softCreditData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.softCreditForm.markAsPristine();
    this.softCreditForm.markAsUntouched();
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


  public userIsSchedulerSoftCreditReader(): boolean {
    return this.softCreditService.userIsSchedulerSoftCreditReader();
  }

  public userIsSchedulerSoftCreditWriter(): boolean {
    return this.softCreditService.userIsSchedulerSoftCreditWriter();
  }
}
