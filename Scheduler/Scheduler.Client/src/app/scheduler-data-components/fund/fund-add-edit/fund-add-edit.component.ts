/*
   GENERATED FORM FOR THE FUND TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Fund table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to fund-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FundService, FundData, FundSubmitData } from '../../../scheduler-data-services/fund.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface FundFormValues {
  name: string,
  description: string | null,
  glCode: string | null,
  isRestricted: boolean,
  goalAmount: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-fund-add-edit',
  templateUrl: './fund-add-edit.component.html',
  styleUrls: ['./fund-add-edit.component.scss']
})
export class FundAddEditComponent {
  @ViewChild('fundModal') fundModal!: TemplateRef<any>;
  @Output() fundChanged = new Subject<FundData[]>();
  @Input() fundSubmitData: FundSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FundFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public fundForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        glCode: [''],
        isRestricted: [false],
        goalAmount: [''],
        notes: [''],
        sequence: [''],
        iconId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  funds$ = this.fundService.GetFundList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private fundService: FundService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(fundData?: FundData) {

    if (fundData != null) {

      if (!this.fundService.userIsSchedulerFundReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Funds`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.fundSubmitData = this.fundService.ConvertToFundSubmitData(fundData);
      this.isEditMode = true;
      this.objectGuid = fundData.objectGuid;

      this.buildFormValues(fundData);

    } else {

      if (!this.fundService.userIsSchedulerFundWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Funds`,
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
        this.fundForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.fundForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.fundModal, {
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

    if (this.fundService.userIsSchedulerFundWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Funds`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.fundForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.fundForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.fundForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const fundSubmitData: FundSubmitData = {
        id: this.fundSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        glCode: formValue.glCode?.trim() || null,
        isRestricted: !!formValue.isRestricted,
        goalAmount: formValue.goalAmount ? Number(formValue.goalAmount) : null,
        notes: formValue.notes?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.fundSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateFund(fundSubmitData);
      } else {
        this.addFund(fundSubmitData);
      }
  }

  private addFund(fundData: FundSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    fundData.versionNumber = 0;
    fundData.active = true;
    fundData.deleted = false;
    this.fundService.PostFund(fundData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newFund) => {

        this.fundService.ClearAllCaches();

        this.fundChanged.next([newFund]);

        this.alertService.showMessage("Fund added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/fund', newFund.id]);
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
                                   'You do not have permission to save this Fund.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Fund.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Fund could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateFund(fundData: FundSubmitData) {
    this.fundService.PutFund(fundData.id, fundData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedFund) => {

        this.fundService.ClearAllCaches();

        this.fundChanged.next([updatedFund]);

        this.alertService.showMessage("Fund updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Fund.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Fund.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Fund could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(fundData: FundData | null) {

    if (fundData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.fundForm.reset({
        name: '',
        description: '',
        glCode: '',
        isRestricted: false,
        goalAmount: '',
        notes: '',
        sequence: '',
        iconId: null,
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.fundForm.reset({
        name: fundData.name ?? '',
        description: fundData.description ?? '',
        glCode: fundData.glCode ?? '',
        isRestricted: fundData.isRestricted ?? false,
        goalAmount: fundData.goalAmount?.toString() ?? '',
        notes: fundData.notes ?? '',
        sequence: fundData.sequence?.toString() ?? '',
        iconId: fundData.iconId,
        color: fundData.color ?? '',
        versionNumber: fundData.versionNumber?.toString() ?? '',
        active: fundData.active ?? true,
        deleted: fundData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.fundForm.markAsPristine();
    this.fundForm.markAsUntouched();
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


  public userIsSchedulerFundReader(): boolean {
    return this.fundService.userIsSchedulerFundReader();
  }

  public userIsSchedulerFundWriter(): boolean {
    return this.fundService.userIsSchedulerFundWriter();
  }
}
