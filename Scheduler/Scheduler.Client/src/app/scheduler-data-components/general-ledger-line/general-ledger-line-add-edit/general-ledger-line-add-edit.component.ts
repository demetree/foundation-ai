/*
   GENERATED FORM FOR THE GENERALLEDGERLINE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from GeneralLedgerLine table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to general-ledger-line-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { GeneralLedgerLineService, GeneralLedgerLineData, GeneralLedgerLineSubmitData } from '../../../scheduler-data-services/general-ledger-line.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { GeneralLedgerEntryService } from '../../../scheduler-data-services/general-ledger-entry.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface GeneralLedgerLineFormValues {
  generalLedgerEntryId: number | bigint,       // For FK link number
  financialCategoryId: number | bigint,       // For FK link number
  debitAmount: string,     // Stored as string for form input, converted to number on submit.
  creditAmount: string,     // Stored as string for form input, converted to number on submit.
  description: string | null,
};

@Component({
  selector: 'app-general-ledger-line-add-edit',
  templateUrl: './general-ledger-line-add-edit.component.html',
  styleUrls: ['./general-ledger-line-add-edit.component.scss']
})
export class GeneralLedgerLineAddEditComponent {
  @ViewChild('generalLedgerLineModal') generalLedgerLineModal!: TemplateRef<any>;
  @Output() generalLedgerLineChanged = new Subject<GeneralLedgerLineData[]>();
  @Input() generalLedgerLineSubmitData: GeneralLedgerLineSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<GeneralLedgerLineFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public generalLedgerLineForm: FormGroup = this.fb.group({
        generalLedgerEntryId: [null, Validators.required],
        financialCategoryId: [null, Validators.required],
        debitAmount: ['', Validators.required],
        creditAmount: ['', Validators.required],
        description: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  generalLedgerLines$ = this.generalLedgerLineService.GetGeneralLedgerLineList();
  generalLedgerEntries$ = this.generalLedgerEntryService.GetGeneralLedgerEntryList();
  financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();

  constructor(
    private modalService: NgbModal,
    private generalLedgerLineService: GeneralLedgerLineService,
    private generalLedgerEntryService: GeneralLedgerEntryService,
    private financialCategoryService: FinancialCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(generalLedgerLineData?: GeneralLedgerLineData) {

    if (generalLedgerLineData != null) {

      if (!this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read General Ledger Lines`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.generalLedgerLineSubmitData = this.generalLedgerLineService.ConvertToGeneralLedgerLineSubmitData(generalLedgerLineData);
      this.isEditMode = true;

      this.buildFormValues(generalLedgerLineData);

    } else {

      if (!this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write General Ledger Lines`,
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
        this.generalLedgerLineForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.generalLedgerLineForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.generalLedgerLineModal, {
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

    if (this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write General Ledger Lines`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.generalLedgerLineForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.generalLedgerLineForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.generalLedgerLineForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const generalLedgerLineSubmitData: GeneralLedgerLineSubmitData = {
        id: this.generalLedgerLineSubmitData?.id || 0,
        generalLedgerEntryId: Number(formValue.generalLedgerEntryId),
        financialCategoryId: Number(formValue.financialCategoryId),
        debitAmount: Number(formValue.debitAmount),
        creditAmount: Number(formValue.creditAmount),
        description: formValue.description?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateGeneralLedgerLine(generalLedgerLineSubmitData);
      } else {
        this.addGeneralLedgerLine(generalLedgerLineSubmitData);
      }
  }

  private addGeneralLedgerLine(generalLedgerLineData: GeneralLedgerLineSubmitData) {
    this.generalLedgerLineService.PostGeneralLedgerLine(generalLedgerLineData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newGeneralLedgerLine) => {

        this.generalLedgerLineService.ClearAllCaches();

        this.generalLedgerLineChanged.next([newGeneralLedgerLine]);

        this.alertService.showMessage("General Ledger Line added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/generalledgerline', newGeneralLedgerLine.id]);
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
                                   'You do not have permission to save this General Ledger Line.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the General Ledger Line.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('General Ledger Line could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateGeneralLedgerLine(generalLedgerLineData: GeneralLedgerLineSubmitData) {
    this.generalLedgerLineService.PutGeneralLedgerLine(generalLedgerLineData.id, generalLedgerLineData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedGeneralLedgerLine) => {

        this.generalLedgerLineService.ClearAllCaches();

        this.generalLedgerLineChanged.next([updatedGeneralLedgerLine]);

        this.alertService.showMessage("General Ledger Line updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this General Ledger Line.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the General Ledger Line.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('General Ledger Line could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(generalLedgerLineData: GeneralLedgerLineData | null) {

    if (generalLedgerLineData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.generalLedgerLineForm.reset({
        generalLedgerEntryId: null,
        financialCategoryId: null,
        debitAmount: '',
        creditAmount: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.generalLedgerLineForm.reset({
        generalLedgerEntryId: generalLedgerLineData.generalLedgerEntryId,
        financialCategoryId: generalLedgerLineData.financialCategoryId,
        debitAmount: generalLedgerLineData.debitAmount?.toString() ?? '',
        creditAmount: generalLedgerLineData.creditAmount?.toString() ?? '',
        description: generalLedgerLineData.description ?? '',
      }, { emitEvent: false});
    }

    this.generalLedgerLineForm.markAsPristine();
    this.generalLedgerLineForm.markAsUntouched();
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


  public userIsSchedulerGeneralLedgerLineReader(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineReader();
  }

  public userIsSchedulerGeneralLedgerLineWriter(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter();
  }
}
